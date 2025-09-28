using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteObjectRelationalMapping<TContext> : ISqliteSchemaObjectRelationalMapping<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    SqliteDbSchemaChanges DetectedSchemaChanges { get; }

    bool CreateDatabaseIfNotExists();
    bool Migrate();
    void DeleteDatabase();
}