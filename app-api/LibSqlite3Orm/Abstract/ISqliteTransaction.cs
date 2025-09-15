namespace LibSqlite3Orm.Abstract;

public interface ISqliteTransaction : IDisposable
{
    event EventHandler Committed;
    event EventHandler RolledBack;
    string Name { get; }
    
    void Commit();
    void Rollback();
}