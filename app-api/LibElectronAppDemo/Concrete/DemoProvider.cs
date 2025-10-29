using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Database;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;

namespace LibElectronAppDemo.Concrete;

public class DemoProvider : IDemoProvider
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<ISqliteObjectRelationalMapperDatabaseManager<MusicManagerDbContext>> dbManagerFactory;
    private readonly IDatabaseSeeder databaseSeeder;

    public DemoProvider(Func<ISqliteConnection> connectionFactory,
        Func<ISqliteObjectRelationalMapperDatabaseManager<MusicManagerDbContext>> dbManagerFactory, IDatabaseSeeder databaseSeeder)
    {
        this.connectionFactory = connectionFactory;
        this.dbManagerFactory = dbManagerFactory;
        this.databaseSeeder = databaseSeeder;
    }

    public string CreateDemoDb(string dbFilename = null)
    {
        var dbManager = dbManagerFactory();
        dbFilename ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "music-man.sqlite");
        using var connection = connectionFactory();
        dbManager.UseConnection(connection);
        connection.OpenReadWrite(dbFilename, mustExist: false);
        dbManager.DeleteDatabase();
        connection.OpenReadWrite(dbFilename, mustExist: false);
        dbManager.CreateDatabase();
        databaseSeeder.SeedDatabase(connection);
        return dbFilename;
    }
}