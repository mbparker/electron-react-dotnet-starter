using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityServices : IEntityServices
{
    private readonly IEntityCreator creator;
    private readonly IEntityGetter getter;
    private readonly IEntityUpdater updater;
    private readonly IEntityDeleter deleter;
    private readonly IEntityUpserter upserter;
    
    public EntityServices(Func<ISqliteOrmDatabaseContext, IEntityCreator> entityCreatorFactory,
        Func<ISqliteOrmDatabaseContext, IEntityUpdater> entityUpdaterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityUpserter> entityUpserterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityGetter> entityGetterFactory,
        Func<ISqliteOrmDatabaseContext, IEntityDeleter> entityDeleterFactory,
        ISqliteOrmDatabaseContext context)
    {
        creator = entityCreatorFactory(context);
        getter = entityGetterFactory(context);
        updater = entityUpdaterFactory(context);
        deleter = entityDeleterFactory(context);
        upserter = entityUpserterFactory(context);
    }

    public bool Insert<T>(T entity)
    {
        return creator.Insert(entity);
    }

    public bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        return creator.Insert(connection, synthesisResult, entity);
    }

    public int InsertMany<T>(IEnumerable<T> entities)
    {
        return creator.InsertMany(entities);
    }

    public ISqliteQueryable<T> Get<T>(bool includeDetails = false) where T : new()
    {
        return getter.Get<T>(includeDetails);
    }

    public bool Update<T>(T entity)
    {
        return updater.Update(entity);
    }

    public bool Update<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity)
    {
        return updater.Update(connection, synthesisResult, entity);
    }

    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        return updater.UpdateMany(entities);
    }

    public int Delete<T>(Expression<Func<T, bool>> predicate)
    {
        return deleter.Delete(predicate);
    }

    public int DeleteAll<T>()
    {
        return deleter.DeleteAll<T>();
    }

    public UpsertResult Upsert<T>(T entity)
    {
        return upserter.Upsert(entity);
    }

    public UpsertManyResult UpsertMany<T>(IEnumerable<T> entities)
    {
        return upserter.UpsertMany(entities);
    }
}