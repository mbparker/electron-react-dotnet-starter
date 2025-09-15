namespace LibSqlite3Orm.Abstract;

public interface ISqliteUniqueIdGenerator
{
    Guid NewGuid();
    string NewUniqueId();
}