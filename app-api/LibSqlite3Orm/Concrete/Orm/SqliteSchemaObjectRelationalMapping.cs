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
    private ISqliteConnection _connection;
    private ISqliteTransaction _transaction;

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
    
    public ISqliteConnection CurrentTransactionConnection => _connection;

    public void BeginTransaction()
    {
        if (_connection is not null) throw new InvalidOperationException("The global transaction has already been started.");
        _connection = connectionFactory();
        _connection.Open(Context.Filename, true);
        _transaction = _connection.BeginTransaction();
    }
    
    public void CommitTransaction()
    {
        if (_transaction is null) throw new InvalidOperationException("The global transaction has not been started.");
        try
        {
            _transaction.Commit();
        }
        finally
        {
            try
            {
                _transaction.Dispose();
            }
            finally
            {
                _transaction = null;  
                try
                {
                    _connection.Dispose();
                }
                finally
                {
                    _connection = null;   
                }                
            }
        }
    }
    
    public void RollbackTransaction()
    {
        if (_transaction is null) throw new InvalidOperationException("The global transaction has not been started.");
        try
        {
            _transaction.Rollback();
        }
        finally
        {
            try
            {
                _transaction.Dispose();
            }
            finally
            {
                _transaction = null;  
                try
                {
                    _connection.Dispose();
                }
                finally
                {
                    _connection = null;   
                }                
            }
        }
    }    

    public int ExecuteNonQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        if (_connection is not null)
        {
            using (var command = _connection.CreateCommand())
            {
                populateParamsAction?.Invoke(command.Parameters);
                return command.ExecuteNonQuery(sql);
            }
        }
        
        using (var connection = connectionFactory())
        {
            connection.Open(Context.Filename, true);
            using (var command = connection.CreateCommand())
            {
                populateParamsAction?.Invoke(command.Parameters);
                return command.ExecuteNonQuery(sql);
            }
        }
    }

    public ISqliteDataReader ExecuteQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        if (_connection is not null)
        {
            using (var command = _connection.CreateCommand())
            {
                populateParamsAction?.Invoke(command.Parameters);
                return command.ExecuteQuery(sql);
            }
        }
        else
        {
            var connection = connectionFactory();
            connection.Open(Context.Filename, true);
            using (var command = connection.CreateCommand())
            {
                populateParamsAction?.Invoke(command.Parameters);
                var reader = command.ExecuteQuery(sql);
                reader.OnDispose += (sender, args) =>
                {
                    connection.Dispose();
                    connection = null;
                };
                return reader;
            }
        }
    }
    
    public bool Insert<T>(T entity)
    {
        if (_connection is not null)
            return EntityServices.Insert(_connection, entity);
        return EntityServices.Insert(entity);
    }
    
    public int InsertMany<T>(IEnumerable<T> entities)
    {
        if (_connection is not null)
            return EntityServices.InsertMany(_connection, entities);
        return EntityServices.InsertMany(entities);
    }
    
    public bool Update<T>(T entity)
    {
        if (_connection is not null)
            return EntityServices.Update(_connection, entity);    
        return EntityServices.Update(entity);
    }
    
    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        if (_connection is not null)
            return EntityServices.UpdateMany(_connection, entities);  
        return EntityServices.UpdateMany(entities);
    }
    
    public UpsertResult Upsert<T>(T entity)
    {
        if (_connection is not null)
            return EntityServices.Upsert(_connection, entity);    
        return EntityServices.Upsert(entity);
    }
    
    public UpsertManyResult UpsertMany<T>(IEnumerable<T> entities)
    {
        if (_connection is not null)
            return EntityServices.UpsertMany(_connection, entities);          
        return EntityServices.UpsertMany(entities);
    }

    public ISqliteQueryable<T> Get<T>(bool loadNavigationProps = false) where T : new()
    {
        if (_connection is not null)
            return EntityServices.Get<T>(_connection, loadNavigationProps);
        return EntityServices.Get<T>(loadNavigationProps);
    }
    
    public int Delete<T>(Expression<Func<T, bool>> predicate)
    {
        if (_connection is not null)
            return EntityServices.Delete(_connection, predicate);
        return EntityServices.Delete(predicate);
    }

    public int DeleteAll<T>()
    {
        if (_connection is not null)
            return EntityServices.DeleteAll<T>(_connection);
        return EntityServices.DeleteAll<T>();
    }
}