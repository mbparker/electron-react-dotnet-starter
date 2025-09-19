using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteEntityWriter
{
    void SetGeneratedKeyOnEntityIfNeeded<T>(SqliteDbSchema schema, ISqliteConnection connection, T entity);
    T Deserialize<T>(SqliteDbSchemaTable table, ISqliteDataRow row, Func<Type, T, object> getDetailsListFunc, Func<Type, T, object> getDetailsFunc) where T : new();
}