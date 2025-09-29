using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteEntityWriter : ISqliteEntityWriter
{
    private readonly ISqliteFieldValueSerialization serialization;
    
    public SqliteEntityWriter(ISqliteFieldValueSerialization serialization)
    {
        this.serialization = serialization;    
    }
    
    public void SetGeneratedKeyOnEntityIfNeeded<T>(SqliteDbSchema schema, ISqliteConnection connection, T entity)
    {
        var type = typeof(T);
        var table = schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == type.AssemblyQualifiedName);
        if (table is not null)
        {
            var autoIncFieldName = table.PrimaryKey?.AutoIncrement ?? false ? table.PrimaryKey.FieldName : null;
            if (!string.IsNullOrWhiteSpace(autoIncFieldName))
            {
                var id = connection.GetLastInsertedId();
                var col = table.Columns[autoIncFieldName];
                var member = type.GetMember(col.ModelFieldName).Single();
                member.SetValue(entity, id);
            }
        }
        else
            throw new InvalidDataContractException($"Type {type.AssemblyQualifiedName} is not mapped in the schema.");
    }

    public TEntity Deserialize<TEntity>(SqliteDbSchemaTable table, ISqliteDataRow row,
        IDetailEntityGetter detailEntityGetter, bool loadNavigationProps) where TEntity : new()
    {
        var entity = new TEntity();
        var entityType = entity.GetType();
        foreach (var col in table.Columns.Values)
        {
            var member = entityType.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                var rowField = row[col.Name];
                rowField.UseSerializer(serialization[Type.GetType(col.ModelFieldTypeName)]);
                member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
            }
        }

        var detailGetterType = typeof(IDetailEntityGetter);
        var getDetailsListGeneric = detailGetterType.GetMethod(nameof(IDetailEntityGetter.GetDetailsList));
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
                        var queryable = getDetailsList.Invoke(detailEntityGetter, [entity, loadNavigationProps]);
                        member.SetValue(entity, queryable);
                    }
                }
            }
        }

        var getDetailsGeneric = detailGetterType.GetMethod(nameof(IDetailEntityGetter.GetDetails));
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
                        var detailEntity = getDetails.Invoke(detailEntityGetter, [entity, loadNavigationProps]);
                        member.SetValue(entity, detailEntity);
                    }
                }
            }
        }

        return entity;
    }
}