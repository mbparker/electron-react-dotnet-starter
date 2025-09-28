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
    private readonly Dictionary<Type, ConstructorInfo> lazyConstructors = new();

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
        return Get<T>(() =>
        {
            var connection = connectionFactory();
            connection.Open(context.Filename, true);
            return connection;
        }, includeDetails, true);
    }

    public ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool includeDetails = false) where T : new()
    {
        return Get<T>(() => connection, includeDetails, false);
    }

    private ISqliteQueryable<T> Get<T>(Func<ISqliteConnection> connectionAllocator, bool includeDetails, bool disposeConnection) where T : new()
    {
        var entityTypeName = typeof(T).AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            ISqliteDataReader ExecuteQuery(SynthesizeSelectSqlArgs args)
            {
                var synthesizer = dmlSqlSynthesizerFactory(SqliteDmlSqlSynthesisKind.Select, context.Schema);
                var synthesisResult = synthesizer.Synthesize<T>(new SqliteDmlSqlSynthesisArgs(args));
                using (var cmd = connectionAllocator().CreateCommand())
                {
                    parameterPopulator.Populate<T>(synthesisResult, cmd.Parameters);
                    return cmd.ExecuteQuery(synthesisResult.SqlText);
                }
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
                
                return CreateLazyQueryableNull(detailTableType);
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

                return CreateLazyNull(detailTableType);
            }            

            T DeserializeRow(ISqliteDataRow row)
            {
                return entityWriter.Deserialize<T>(table, row, GetDetailsListPropertyValue, GetDetailsPropertyValue);
            }
            
            return new SqliteOrderedQueryable<T>(ExecuteQuery, DeserializeRow, disposeConnection);
        }
        
        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }

    private object CreateLazyQueryableNull(Type type)
    {
        return CreateLazyNull(typeof(ISqliteQueryable<>).MakeGenericType(type));
    }

    private object CreateLazyNull(Type type)
    {
        if (!lazyConstructors.TryGetValue(type, out var ctor))
        {
            var lazyType = typeof(Lazy<>).MakeGenericType(type);
            ctor = lazyType.GetConstructors().FirstOrDefault(x =>
            {
                var p = x.GetParameters();
                return p.Length == 1 && p[0].ParameterType == type;
            });
            lazyConstructors.Add(type, ctor);
        }
        
        return ctor?.Invoke([null]);
    }

    private Lazy<TDetails> GetDetails<TTable, TDetails>(TTable record) where TDetails : new()
    {
        return new Lazy<TDetails>(() =>
        {
            // Initialize the queryable, then build an expression in code to filter the results to the current record.
            // Do not fetch detail records in order to help prevent infinite recursion in apps using this library.
            var queryable = Get<TDetails>(includeDetails: false);
            var tableType = typeof(TTable);
            var detailTableType = typeof(TDetails);
            var entityTypeName = detailTableType.AssemblyQualifiedName;
            var masterTable =
                context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == tableType.AssemblyQualifiedName);
            var detailTable = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
            if (detailTable is not null && masterTable is not null)
            {
                var whereMethod = queryable.GetType().GetMethod(nameof(ISqliteQueryable<TDetails>.Where));
                if (whereMethod is not null)
                {
                    var fks = masterTable.ForeignKeys
                        .Where(x => x.ForeignTableModelTypeName == detailTableType.AssemblyQualifiedName)
                        .ToArray();
                    Expression<Func<TDetails, bool>> wherePredicate = null;
                    foreach (var fk in fks)
                    {
                        for (var i = 0; i < fk.FieldNames.Length; i++)
                        {
                            // This is from the perspective of the details table, since were getting our info from the foreign key specs.
                            // Examples in plain text:
                            // x => x.Id1 == 1234
                            // x => x.Id1 == 1234 && x.Id2 == 5678 
                            //
                            // Get the member info for both sides. On the master table, we are using a member access expression to build a where clause later.
                            var masterMemberInfo = tableType.GetMember(fk.FieldNames[i]).First();
                            // On the detail side, we will use it to get a live value.
                            var detailsMemberInfo = detailTableType.GetMember(fk.ForeignTableFields[i]).First();
                            // Define "x" with a param expression.
                            var detailTableParamExpr = Expression.Parameter(detailTableType, "x");
                            // Specify what member on "x" we are accessing
                            var detailsMemberExpr =
                                Expression.MakeMemberAccess(detailTableParamExpr, detailsMemberInfo);
                            // Create a constant value expression for comparison against
                            var masterValueConstExpr = Expression.Constant(masterMemberInfo.GetValue(record),
                                masterMemberInfo.GetValueType());
                            // Now build the equals binary expression
                            var equalComparisonExpr = Expression.MakeBinary(ExpressionType.Equal, detailsMemberExpr,
                                masterValueConstExpr);
                            // Lastly, wrap the binary expression in a lambda expression to match the Where method's input predicate type
                            var lambdaWrapperExpr =
                                Expression.Lambda<Func<TDetails, bool>>(equalComparisonExpr, detailTableParamExpr);
                            // Set the predicate expression. If we already set one, link them together with a logical AND expression.
                            if (wherePredicate is null)
                                wherePredicate = lambdaWrapperExpr;
                            else
                                wherePredicate =
                                    Expression.Lambda<Func<TDetails, bool>>(Expression.AndAlso(wherePredicate.Body,
                                        lambdaWrapperExpr.Body), detailTableParamExpr);
                        }
                    }

                    // Now that we've built the predicate we can manually invoke the Where function which will build the where clause when the queryable is enumerated.
                    // Then we take what should be a single record.
                    if (whereMethod.Invoke(queryable, [wherePredicate]) is ISqliteQueryable<TDetails> enumerable)
                        return enumerable.AsEnumerable().SingleOrDefault();
                    return default;
                }
            }

            throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
        });
    }

    private Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TTable, TDetails>(TTable record) where TDetails : new()
    {
        return new Lazy<ISqliteQueryable<TDetails>>(() =>
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
                    var fks = detailTable.ForeignKeys
                        .Where(x => x.ForeignTableModelTypeName == tableType.AssemblyQualifiedName)
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
                            var detailsMemberExpr =
                                Expression.MakeMemberAccess(detailTableParamExpr, detailsMemberInfo);
                            // Create a constant value expression for comparison against
                            var masterValueConstExpr = Expression.Constant(masterMemberInfo.GetValue(record),
                                masterMemberInfo.GetValueType());
                            // Now build the equals binary expression
                            var equalComparisonExpr = Expression.MakeBinary(ExpressionType.Equal, detailsMemberExpr,
                                masterValueConstExpr);
                            // Lastly, wrap the binary expression in a lambda expression to match the Where method's input predicate type
                            var lambdaWrapperExpr =
                                Expression.Lambda<Func<TDetails, bool>>(equalComparisonExpr, detailTableParamExpr);
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
        });
    }
}