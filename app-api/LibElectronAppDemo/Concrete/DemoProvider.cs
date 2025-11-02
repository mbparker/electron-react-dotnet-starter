using LibElectronAppApi.Shared;
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
    private readonly IFileOperations fileOperations;

    public DemoProvider(Func<ISqliteConnection> connectionFactory,
        Func<ISqliteObjectRelationalMapperDatabaseManager<MusicManagerDbContext>> dbManagerFactory, IDatabaseSeeder databaseSeeder,
        IFileOperations fileOperations)
    {
        this.connectionFactory = connectionFactory;
        this.dbManagerFactory = dbManagerFactory;
        this.databaseSeeder = databaseSeeder;
        this.fileOperations = fileOperations;
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
        var exists = DatabaseExistsAndIsInitialized(dbFilename);

        long totalWork = 6; // Initial calls to ReportProgress
        long completedWork = 0;

        void ReportProgress(string step, bool done = false)
        {
            if (!exists || dropExisting)
                progressHandler?.ReportInteractiveTaskProgress("Creating database...", step, totalWork,
                    done ? totalWork : completedWork++);
        }

        ReportProgress("Initializing");
        try
        {
            using var dbManager = dbManagerFactory();
            dbFilename = ResolveDbFilename(dbFilename);
            using var connection = connectionFactory();
            dbManager.UseConnection(connection);
            if (dropExisting)
            {
                ReportProgress("Removing existing database");
                connection.OpenReadWrite(dbFilename, mustExist: false);
                dbManager.DeleteDatabase();
                ReportProgress("Existing database dropped");
            }

            ReportProgress("Connect to new database");
            connection.OpenReadWrite(dbFilename, mustExist: false);
            if (!exists || dropExisting)
            {
                ReportProgress("Create database schema");
                dbManager.CreateDatabase();
                databaseSeeder.SeedDatabase(progressHandler, connection, ref totalWork, completedWork);
            }
        }
        finally
        {
            ReportProgress("Done", true);
        }
    }

    private bool DatabaseExistsAndIsInitialized(string dbFilename = null)
    {
        var testConnection = TryConnectToDemoDb(ResolveDbFilename(dbFilename));
        var dbExists = testConnection is not null;
        testConnection?.Dispose();
        return dbExists;
    }

    private string ResolveDbFilename(string requestedFilename = null)
    {
        var result = requestedFilename ?? Path.Combine(fileOperations.GetLocalAppDataPathForCurrentPlatform(), 
            SharedConstants.AppName, "music-man.sqlite");
        var dir = Path.GetDirectoryName(result);
        if (string.IsNullOrWhiteSpace(dir))
            throw new ApplicationException("Unable to determine directory name for database storage.");
        if (!fileOperations.DirectoryExists(dir))
            fileOperations.CreateDirectory(dir);
        return result;
    }
}