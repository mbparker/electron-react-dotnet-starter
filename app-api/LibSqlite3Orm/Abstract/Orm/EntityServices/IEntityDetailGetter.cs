namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDetailGetter
{
    Lazy<TDetails> GetDetails<TEntity, TDetails>(TEntity record, bool loadNavigationProps = false, ISqliteConnection connection = null)
        where TDetails : new();    
    Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TEntity, TDetails>(TEntity record, bool loadNavigationProps = false, ISqliteConnection connection = null)
        where TDetails : new();    
} 