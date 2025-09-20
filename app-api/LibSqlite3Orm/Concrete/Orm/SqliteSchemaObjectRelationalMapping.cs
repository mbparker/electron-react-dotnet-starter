using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteSchemaObjectRelationalMapping<TContext> : ISqliteSchemaObjectRelationalMapping<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<TContext> contextFactory;
    private readonly Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory;
    private TContext _context;
    private IEntityServices _entityServices;

    public SqliteSchemaObjectRelationalMapping(Func<ISqliteConnection> connectionFactory, Func<TContext> contextFactory,
        Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory)
    {
        this.connectionFactory = connectionFactory;
        this.contextFactory = contextFactory;
        this.entityServicesFactory = entityServicesFactory;
    }

    public TContext Context
    {
        get
        {
            if (_context is null)
                _context = contextFactory();
            return _context;
        }
    }
    
    private IEntityServices EntityServices
    {
        get
        {
            if (_entityServices is null)
                _entityServices = entityServicesFactory(Context);
            return _entityServices;
        }
    }

    public int ExecuteNonQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        using (var connection = connectionFactory())
        {
            connection.Open(Context.Filename, true);
            var command = connection.CreateCommand();
            populateParamsAction?.Invoke(command.Parameters);
            return command.ExecuteNonQuery(sql);
        }
    }

    public ISqliteDataReader ExecuteQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        var connection = connectionFactory();
        connection.Open(Context.Filename, true);
        var command = connection.CreateCommand();
        populateParamsAction?.Invoke(command.Parameters);
        var reader = command.ExecuteQuery(sql);
        reader.OnDispose += (sender, args) =>
        {
            connection.Dispose();
            connection = null;
        };
        return reader;
    }
    
    public bool Insert<T>(T entity)
    {
        return EntityServices.Insert(entity);
    }
    
    public int InsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.InsertMany(entities);
    }
    
    public bool Update<T>(T entity)
    {
        return EntityServices.Update(entity);
    }
    
    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpdateMany(entities);
    }
    
    public UpsertResult Upsert<T>(T entity)
    {
        return EntityServices.Upsert(entity);
    }
    
    public UpsertManyResult UpsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpsertMany(entities);
    }

    public ISqliteQueryable<T> Get<T>(bool includeDetails = false) where T : new()
    {
        return EntityServices.Get<T>(includeDetails);
    }
    
    public int Delete<T>(Expression<Func<T, bool>> predicate)
    {
        return EntityServices.Delete(predicate);
    }

    public int DeleteAll<T>()
    {
        return EntityServices.DeleteAll<T>();
    }
}