using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteEntityWriter
{
    void SetGeneratedKeyOnEntityIfNeeded<T>(SqliteDbSchema schema, ISqliteConnection connection, T entity);
    TEntity Deserialize<TEntity>(SqliteDbSchemaTable table, ISqliteDataRow row, IDetailEntityGetter detailEntityGetter,
        bool loadNavigationProps) where TEntity : new();
}