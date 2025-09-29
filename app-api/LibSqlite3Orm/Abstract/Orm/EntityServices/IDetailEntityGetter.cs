namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

// This interface is not registered because it is only used internally. It is implemented by EntityGetter,
// which is registered as IEntityGetter. There is a mutual dependency between EntityGetter and SqliteEntityWriter
// which necessitates this. ISqliteEntityWriter gets ctor injected into EntityGetter, which must pass itself
// (as IDetailEntityGetter) into the Deserialize method of ISqliteEntityWriter.
public interface IDetailEntityGetter
{
    Lazy<TDetails> GetDetails<TEntity, TDetails>(TEntity record, bool loadNavigationProps = false)
        where TDetails : new();
    Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TEntity, TDetails>(TEntity record, bool loadNavigationProps = false)
        where TDetails : new();
} 