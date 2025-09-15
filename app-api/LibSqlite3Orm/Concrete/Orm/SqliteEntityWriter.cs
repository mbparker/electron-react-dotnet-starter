using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteEntityWriter : ISqliteEntityWriter
{
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

    public T Deserialize<T>(SqliteDbSchemaTable table, ISqliteDataRow row, Func<Type, T, object> getDetailsFunc) where T : new()
    {
        var entity = new T();
        var type = entity.GetType();
        foreach (var col in table.Columns.Values)
        {
            var member = type.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                var rowField = row[col.Name];
                if (!string.IsNullOrWhiteSpace(col.ConverterTypeName))
                    rowField.UseConverter(Type.GetType(col.ConverterTypeName));
                member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
            }
        }

        if (getDetailsFunc is not null)
        {
            foreach (var detailsProp in table.DetailProperties)
            {
                var member = type.GetMember(detailsProp.DetailsListPropertyName).SingleOrDefault();
                if (member is not null)
                {
                    var tableType = Type.GetType(detailsProp.DetailTableTypeName);
                    var queryable = getDetailsFunc(tableType, entity);
                    member.SetValue(entity, queryable);
                }
            }
        }

        return entity;
    }
}