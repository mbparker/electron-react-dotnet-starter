namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityGetter
{
    ISqliteQueryable<T> Get<T>(bool loadNavigationProps = false) where T : new();
    ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool loadNavigationProps = false) where T : new();
}