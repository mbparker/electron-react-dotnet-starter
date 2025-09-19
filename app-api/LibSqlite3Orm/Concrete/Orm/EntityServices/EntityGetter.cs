using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityGetter : IEntityGetter
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory;
    private readonly ISqliteParameterPopulator  parameterPopulator;
    private readonly ISqliteEntityWriter entityWriter;
    private readonly ISqliteOrmDatabaseContext context;

    public EntityGetter(Func<ISqliteConnection> connectionFactory,
        Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> dmlSqlSynthesizerFactory,
        ISqliteParameterPopulator  parameterPopulator, ISqliteEntityWriter entityWriter, ISqliteOrmDatabaseContext context)
    {
        this.connectionFactory = connectionFactory;
        this.dmlSqlSynthesizerFactory = dmlSqlSynthesizerFactory;
        this.parameterPopulator = parameterPopulator;
        this.entityWriter = entityWriter;
        this.context = context;
    }
    
    public ISqliteQueryable<T> Get<T>(bool includeDetails = false) where T : new()
    {
        var entityTypeName = typeof(T).AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            ISqliteDataReader ExecuteQuery(SynthesizeSelectSqlArgs args)
            {
                var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Select, context.Schema);
                var synthesisResult = synthesizer.Synthesize<T>(new SqliteDmlSqlSynthesisArgs(args));
                
                // The enumerator will dispose of the connection and reader when it finishes the enumeration.
                var connection = connectionFactory();
                connection.Open(context.Filename, true);
                var cmd = connection.CreateCommand();
                parameterPopulator.Populate<T>(synthesisResult, cmd.Parameters, default);
                return cmd.ExecuteQuery(synthesisResult.SqlText);
            }
            
            object GetDetailsListPropertyValue(Type detailTableType, T recordEntity)
            {
                if (includeDetails)
                {
                    MethodInfo getDetailsListMethodGeneric = null;
                    var getDetailsMethod = GetType().GetMethod(nameof(GetDetailsList),
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (getDetailsMethod is not null)
                        getDetailsListMethodGeneric = getDetailsMethod.MakeGenericMethod(typeof(T), detailTableType);
                    if (getDetailsListMethodGeneric is not null)
                        return getDetailsListMethodGeneric.Invoke(this, [recordEntity]);
                }

                return null;
            }
            
            object GetDetailsPropertyValue(Type detailTableType, T recordEntity)
            {
                if (includeDetails)
                {
                    MethodInfo getDetailsListMethodGeneric = null;
                    var getDetailsMethod = GetType().GetMethod(nameof(GetDetails),
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (getDetailsMethod is not null)
                        getDetailsListMethodGeneric = getDetailsMethod.MakeGenericMethod(typeof(T), detailTableType);
                    if (getDetailsListMethodGeneric is not null)
                        return getDetailsListMethodGeneric.Invoke(this, [recordEntity]);
                }

                return null;
            }            

            T DeserializeRow(ISqliteDataRow row)
            {
                return entityWriter.Deserialize<T>(table, row, GetDetailsListPropertyValue, GetDetailsPropertyValue);
            }
            
            return new SqliteOrderedQueryable<T>(ExecuteQuery, DeserializeRow);
        }
        
        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }

    private TDetails GetDetails<TTable, TDetails>(TTable record) where TDetails : new()
    {
        return GetDetailsList<TTable, TDetails>(record).AsEnumerable().SingleOrDefault();
    }

    private ISqliteQueryable<TDetails> GetDetailsList<TTable, TDetails>(TTable record) where TDetails : new()
    {
        // Initialize the queryable recursively, then build an expression in code to filter the results to the current record.
        var queryable = Get<TDetails>(includeDetails: true);
        var tableType = typeof(TTable);
        var detailTableType = typeof(TDetails);
        var entityTypeName = detailTableType.AssemblyQualifiedName;
        var detailTable = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (detailTable is not null)
        {
            var whereMethod = queryable.GetType().GetMethod(nameof(ISqliteQueryable<TDetails>.Where));
            if (whereMethod is not null)
            {
                var fks = detailTable.ForeignKeys.Where(x => x.ForeignTableModelTypeName == tableType.AssemblyQualifiedName)
                    .ToArray();
                Expression<Func<TDetails, bool>> wherePredicate = null;
                foreach (var fk in fks)
                {
                    for (var i = 0; i < fk.FieldNames.Length; i++)
                    {
                        // This is from the perspective of the details table, since were getting our info from the foreign key specs.
                        // Examples in plain text:
                        // x => x.ForeignId == 1234
                        // x => x.ForeignId1 == 1234 && x.ForeignId2 == 5678 
                        //
                        // Get the member info for both sides. On the master table, we will use it to get a live value.
                        var masterMemberInfo = tableType.GetMember(fk.ForeignTableFields[i]).First();
                        // On the detail side, we are using a member access expression to build a where clause later.
                        var detailsMemberInfo = detailTableType.GetMember(fk.FieldNames[i]).First();
                        // Define "x" with a param expression.
                        var detailTableParamExpr = Expression.Parameter(detailTableType, "x");
                        // Specify what member on "x" we are accessing
                        var detailsMemberExpr = Expression.MakeMemberAccess(detailTableParamExpr, detailsMemberInfo);
                        // Create a constant value expression for comparison against
                        var masterValueConstExpr = Expression.Constant(masterMemberInfo.GetValue(record), masterMemberInfo.GetValueType());
                        // Now build the equals binary expression
                        var equalComparisonExpr = Expression.MakeBinary(ExpressionType.Equal, detailsMemberExpr, masterValueConstExpr);
                        // Lastly, wrap the binary expression in a lambda expression to match the Where method's input predicate type
                        var lambdaWrapperExpr = Expression.Lambda<Func<TDetails, bool>>(equalComparisonExpr, detailTableParamExpr);
                        // Set the predicate expression. If we already set one, link them together with a logical AND expression.
                        if (wherePredicate is null) 
                            wherePredicate = lambdaWrapperExpr;
                        else
                            wherePredicate =
                                Expression.Lambda<Func<TDetails, bool>>(Expression.AndAlso(wherePredicate.Body,
                                    lambdaWrapperExpr.Body), detailTableParamExpr);
                    }
                }

                // Now that we've built the predicate we can manually invoke the Where function which will build the where clause when the queryable is finally enumerated.
                return whereMethod.Invoke(queryable, [wherePredicate]) as ISqliteQueryable<TDetails>;
            }
        }
        
        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}