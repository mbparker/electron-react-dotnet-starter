using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityDetailGetter : IEntityDetailGetter
{
    private readonly ISqliteOrmDatabaseContext context;
    private readonly Lazy<IEntityGetter> entityGetter;
    private readonly Lazy<ISqliteDetailPropertyLoader> detailPropertyLoader;

    public EntityDetailGetter(Func<ISqliteOrmDatabaseContext, IEntityGetter> entityGetterFactory,
        Func<ISqliteOrmDatabaseContext, ISqliteDetailPropertyLoader> detailPropertyLoaderFactory, 
        ISqliteOrmDatabaseContext context)
    {
        this.context = context;
        // This Lazy load breaks the circular dependency that exists via SqlEntityWriter.
        entityGetter = new Lazy<IEntityGetter>(() => entityGetterFactory(this.context));
        detailPropertyLoader = new  Lazy<ISqliteDetailPropertyLoader>(() => detailPropertyLoaderFactory(this.context));
    }

    public Lazy<TDetails> GetDetails<TTable, TDetails>(TTable record, bool loadNavigationProps,
        ISqliteDataRow row, ISqliteConnection connection) where TDetails : new()
    {
        return GetDetailsFromRow<TDetails>(loadNavigationProps, row, connection) ??
               GetDetailsFromNewQuery<TTable, TDetails>(record, loadNavigationProps, connection);
    }
    
    public Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TTable, TDetails>(TTable record, bool loadNavigationProps, ISqliteConnection connection) where TDetails : new()
    {
        if (!loadNavigationProps || connection is null)
        {
            return new Lazy<ISqliteQueryable<TDetails>>(default(ISqliteQueryable<TDetails>));
        }
        
        return new Lazy<ISqliteQueryable<TDetails>>(() =>
        {
            // Initialize the queryable recursively, then build an expression in code to filter the results to the current record.
            var queryable = entityGetter.Value.Get<TDetails>(connection, loadNavigationProps: true); 
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
    
        private Lazy<TEntity> GetDetailsFromRow<TEntity>(bool loadNavigationProps, ISqliteDataRow row, ISqliteConnection connection) where TEntity : new()
    {
        if (!loadNavigationProps || connection is null)
        {
            return new Lazy<TEntity>(default(TEntity));
        }
        
        var entityType = typeof(TEntity);
        var entityTypeName = entityType.AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);

        if (table is not null)
        {
            var cols = table
                .Columns
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToArray();
            if (cols.Any(x => row[table.Name + x.Name] is null))
                return null;
            return new Lazy<TEntity>(() =>
            {
                var entity =
                    entityType
                        .GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, [])
                        ?.Invoke(null) ??
                    throw new ApplicationException(
                        $"Cannot locate constructor on type {entityType.AssemblyQualifiedName}");
                foreach (var col in cols)
                {
                    var member = entityType.GetMember(col.ModelFieldName).SingleOrDefault();
                    if (member is not null)
                    {
                        var rowField = row[table.Name + col.Name];
                        member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
                    }
                }

                detailPropertyLoader.Value.LoadDetailProperties(entity, table, row, loadNavigationProps, connection);

                return (TEntity)entity;
            });
        }

        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
    
    private Lazy<TDetails> GetDetailsFromNewQuery<TTable, TDetails>(TTable record, bool loadNavigationProps, ISqliteConnection connection) where TDetails : new()
    {
        if (!loadNavigationProps || connection is null)
        {
            return new Lazy<TDetails>(default(TDetails));
        }
        
        return new Lazy<TDetails>(() =>
        {
            // Initialize the queryable, then build an expression in code to filter the results to the current record.
            var queryable = entityGetter.Value.Get<TDetails>(connection, true);
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
}