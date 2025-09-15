using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteTransaction : ISqliteTransaction
{
    private ISqliteConnection connection;
    
    public SqliteTransaction(ISqliteConnection connection, ISqliteUniqueIdGenerator uniqueIdGenerator)
    {
        this.connection = connection;
        Name = uniqueIdGenerator.NewUniqueId();
        var cmd = this.connection.CreateCommand();
        cmd.ExecuteNonQuery($"SAVEPOINT '{Name}';");
    }

    ~SqliteTransaction()
    {
        Dispose();
    }

    public event EventHandler Committed;
    public event EventHandler RolledBack;
    
    public string Name { get; }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (connection is not null)
            Rollback();
    }
    
    public void Commit()
    {
        if (connection is null) throw new InvalidOperationException("Transaction has already been disposed.");
        var cmd = connection.CreateCommand();
        cmd.ExecuteNonQuery($"RELEASE SAVEPOINT '{Name}';");
        connection = null;
        Committed?.Invoke(this, EventArgs.Empty);
    }

    public void Rollback()
    {
        if (connection is null) throw new InvalidOperationException("Transaction has already been disposed.");
        var cmd = connection.CreateCommand();
        cmd.ExecuteNonQuery($"ROLLBACK TRANSACTION TO SAVEPOINT '{Name}';");
        connection = null;
        RolledBack?.Invoke(this, EventArgs.Empty);
    }
}