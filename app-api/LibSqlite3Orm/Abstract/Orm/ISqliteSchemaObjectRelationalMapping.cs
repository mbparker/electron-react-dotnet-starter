using System.Linq.Expressions;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteSchemaObjectRelationalMapping<TContext> where TContext : ISqliteOrmDatabaseContext
{
    TContext Context { get; }

    bool Insert<T>(T entity);
    int InsertMany<T>(IEnumerable<T> entities);
    bool Update<T>(T entity);
    int UpdateMany<T>(IEnumerable<T> entities);
    UpsertResult Upsert<T>(T entity);
    UpsertManyResult UpsertMany<T>(IEnumerable<T> entities);
    ISqliteQueryable<T> Get<T>(bool includeDetails = false) where T : new();
    int Delete<T>(Expression<Func<T, bool>> predicate);
    int DeleteAll<T>();
}