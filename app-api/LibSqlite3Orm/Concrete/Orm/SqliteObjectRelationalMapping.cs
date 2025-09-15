using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteObjectRelationalMapping<TContext> : SqliteSchemaObjectRelationalMapping<TContext>, ISqliteObjectRelationalMapping<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly ISqliteDbSchemaMigrator<TContext> migrator;
    private readonly ISqliteFileOperations fileOperations;
    private readonly ISqliteDbFactory dbFactory;

    public SqliteObjectRelationalMapping(Func<TContext> contextFactory,
        Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory,
        ISqliteDbSchemaMigrator<TContext> migrator,
        ISqliteFileOperations fileOperations,
        ISqliteDbFactory dbFactory)
        : base(contextFactory, entityServicesFactory)
    {
        this.migrator = migrator;
        this.fileOperations = fileOperations;
        this.dbFactory = dbFactory;
    }

    public SqliteDbSchemaChanges DetectedSchemaChanges { get; private set; } = new();

    public void CreateDatabase()
    {
        dbFactory.Create(Context.Schema , Context.Filename, false);
        migrator.CreateInitialMigration();
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
        if (fileOperations.FileExists(Context.Filename))
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DELETING DATABASE!!");
            Console.ForegroundColor = color;
            fileOperations.DeleteFile(Context.Filename);
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