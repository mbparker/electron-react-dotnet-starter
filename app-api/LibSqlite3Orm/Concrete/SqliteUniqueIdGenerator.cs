using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteUniqueIdGenerator : ISqliteUniqueIdGenerator
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }

    public string NewUniqueId()
    {
        return NewGuid().ToString("N");
    }
}