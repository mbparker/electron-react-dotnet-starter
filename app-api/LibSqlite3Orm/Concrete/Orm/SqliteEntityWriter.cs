using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteEntityWriter : ISqliteEntityWriter
{
    private readonly IEntityDetailGetter entityDetailGetter;

    public SqliteEntityWriter(Func<ISqliteOrmDatabaseContext, IEntityDetailGetter> entityDetailGetterFactory,
        ISqliteOrmDatabaseContext context)
    {
        entityDetailGetter = entityDetailGetterFactory(context);
    }

    public TEntity Deserialize<TEntity>(SqliteDbSchemaTable table, ISqliteDataRow row) where TEntity : new()
    {
        return Deserialize<TEntity>(table, row, false, null);
    }

    public TEntity Deserialize<TEntity>(SqliteDbSchemaTable table, ISqliteDataRow row, bool loadNavigationProps, ISqliteConnection connection) where TEntity : new()
    {
        var entity = new TEntity();
        var entityType = entity.GetType();
        foreach (var col in table.Columns.Values)
        {
            var member = entityType.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                var rowField = row[col.Name];
                member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
            }
        }

        var detailGetterType = typeof(IEntityDetailGetter);
        var getDetailsListGeneric = detailGetterType.GetMethod(nameof(IEntityDetailGetter.GetDetailsList));
        if (getDetailsListGeneric is not null)
        {
            foreach (var detailsProp in table.DetailListProperties)
            {
                var member = entityType.GetMember(detailsProp.DetailsListPropertyName).SingleOrDefault();
                if (member is not null)
                {
                    var detailEntityType = Type.GetType(detailsProp.DetailTableTypeName);
                    if (detailEntityType is not null)
                    {
                        var getDetailsList =
                            getDetailsListGeneric.MakeGenericMethod(entityType, detailEntityType);
                        var queryable = getDetailsList.Invoke(entityDetailGetter, [entity, loadNavigationProps, connection]);
                        member.SetValue(entity, queryable);
                    }
                }
            }
        }

        var getDetailsGeneric = detailGetterType.GetMethod(nameof(IEntityDetailGetter.GetDetails));
        if (getDetailsGeneric is not null)
        {
            foreach (var detailsProp in table.DetailProperties)
            {
                var member = entityType.GetMember(detailsProp.DetailsPropertyName).SingleOrDefault();
                if (member is not null)
                {
                    var detailEntityType = Type.GetType(detailsProp.DetailTableTypeName);
                    if (detailEntityType is not null)
                    {
                        var getDetails =
                            getDetailsGeneric.MakeGenericMethod(entityType, detailEntityType);
                        var detailEntity = getDetails.Invoke(entityDetailGetter, [entity, loadNavigationProps, connection]);
                        member.SetValue(entity, detailEntity);
                    }
                }
            }
        }

        return entity;
    }
}