using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityUpserter
{
    UpsertResult Upsert<T>(T entity);
    UpsertResult Upsert<T>(ISqliteConnection connection, T entity);
    UpsertManyResult UpsertMany<T>(IEnumerable<T> entities);
    UpsertManyResult UpsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities);
}