using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteDbSchemaMigrator<TContext> where TContext : ISqliteOrmDatabaseContext
{
    void CreateInitialMigration();
    
    SqliteDbSchemaChanges CheckForSchemaChanges();

    void Migrate(SqliteDbSchemaChanges changes);
}