using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteObjectRelationalMapperDatabaseManager<TContext> : IDisposable where TContext : ISqliteOrmDatabaseContext
{
    string Filename { get; set; }
    SqliteDbSchemaChanges DetectedSchemaChanges { get; }
    
    void SetConnection(ISqliteConnection connection);
    ISqliteConnection GetConnection();

    void CreateInMemoryDatabase();
    bool CreateDatabase(bool ifNotExists);
    bool Migrate();
    void DeleteDatabase();
}