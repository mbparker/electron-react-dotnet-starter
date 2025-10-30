using LibElectronAppApi.Shared.Abstract;
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

    public ISqliteConnection TryConnectToDemoDb(string dbFilename = null)
    {
        using var dbManager = dbManagerFactory();
        dbFilename = ResolveDbFilename(dbFilename);
        var connection = connectionFactory();
        dbManager.UseConnection(connection);
        try
        {
            connection.OpenReadWrite(dbFilename, mustExist: true);
            if (dbManager.IsDatabaseInitialized())
                return connection;
            connection.Dispose();
            return null;
        }
        catch (Exception)
        {
            connection.Dispose();
            return null;
        }
    }

    public void CreateDemoDb(IBackgroundTaskProgressHandler progressHandler, bool dropExisting = false, string dbFilename = null)
    {
        using var dbManager = dbManagerFactory();
        dbFilename = ResolveDbFilename(dbFilename);
        using var connection = connectionFactory();
        dbManager.UseConnection(connection);
        if (dropExisting)
        {
            connection.OpenReadWrite(dbFilename, mustExist: false);
            dbManager.DeleteDatabase();
        }

        connection.OpenReadWrite(dbFilename, mustExist: false);
        if (!dbManager.IsDatabaseInitialized())
        {
            dbManager.CreateDatabase();
            databaseSeeder.SeedDatabase(progressHandler, connection);
        }
    }

    private string ResolveDbFilename(string requestedFilename = null)
    {
        return requestedFilename ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "music-man.sqlite");        
    }
}