using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteObjectRelationalMapperDatabaseManager<TContext> : ISqliteObjectRelationalMapperDatabaseManager<TContext>
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly ISqliteFileOperations fileOperations;
    private readonly ISqliteDbFactory dbFactory;
    private readonly Func<TContext> contextFactory;
    private ISqliteDbSchemaMigrator<TContext> migrator;
    private TContext _context;
    private string _filename;

    public SqliteObjectRelationalMapperDatabaseManager(
        Func<TContext> contextFactory,
        Func<ISqliteDbSchemaMigrator<TContext>> migratorFactory,
        ISqliteFileOperations fileOperations,
        ISqliteDbFactory dbFactory)
    {
        this.fileOperations = fileOperations;
        this.dbFactory = dbFactory;
        this.contextFactory = contextFactory;
        migrator = migratorFactory();
    }
    
    public string Filename
    {
        get => _filename;
        set => migrator.Filename = _filename = value;
    }
    
    private TContext Context
    {
        get
        {
            if (_context is null)
                _context = contextFactory();
            return _context;
        }
    }

    public SqliteDbSchemaChanges DetectedSchemaChanges { get; private set; } = new();

    public void SetConnection(ISqliteConnection connection)
    {
        migrator.SetConnection(connection);
    }
    
    public ISqliteConnection GetConnection()
    {
        return migrator.GetConnection();
    }

    public void Dispose()
    {
        migrator?.Dispose();
        migrator = null;
    }

    public void CreateInMemoryDatabase()
    {
        //dbFactory.CreateInMemory(Context.Schema);
        migrator.CreateInitialMigration();
    }

    public bool CreateDatabase(bool ifNotExists)
    {
        if (!ifNotExists || !fileOperations.FileExists(Filename))
        {
            dbFactory.Create(Context.Schema, Filename, false);
            migrator.CreateInitialMigration();
            return true;
        }
        
        return false;
    }

    public bool Migrate()
    {
        DetectedSchemaChanges = migrator.CheckForSchemaChanges();
        ThrowIfManualMigrationRequired();
        if (!DetectedSchemaChanges.MigrationRequired) return false;
        migrator.Migrate(DetectedSchemaChanges);
        return true;
    }

    public void DeleteDatabase()
    {
        if (fileOperations.FileExists(Filename))
        {
            ConsoleLogger.WriteLine(ConsoleColor.Red, "DELETING DATABASE!!");
            fileOperations.DeleteFile(Filename);
        }
    }
    
    private void ThrowIfManualMigrationRequired()
    {
        if (DetectedSchemaChanges.ManualMigrationRequired)
        {
            var reasons = string.Join('\n',
                DetectedSchemaChanges.NonMigratableAlteredColumns.Select(x =>
                    $"{x.TableName}.{x.ColumnName}: {x.Reason}"));
            throw new InvalidDataException(
                $"The database cannot be automatically migrated for the following reason(s):\n\n{reasons}\n\nManual migration is required.");
        }
    }
}