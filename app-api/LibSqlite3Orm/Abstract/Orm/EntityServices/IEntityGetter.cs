namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityGetter
{
    ISqliteQueryable<T> Get<T>(bool includeDetails = false) where T : new();
    ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool includeDetails = false) where T : new();
}