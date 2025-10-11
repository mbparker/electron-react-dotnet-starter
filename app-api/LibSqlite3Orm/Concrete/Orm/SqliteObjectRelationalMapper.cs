using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteObjectRelationalMapper<TContext> : ISqliteObjectRelationalMapper<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly Func<ISqliteConnection> connectionFactory;
    private readonly Func<TContext> contextFactory;
    private readonly Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory;
    private TContext _context;
    private IEntityServices _entityServices;
    private ISqliteConnection _connection;
    private ISqliteTransaction _transaction;

    public SqliteObjectRelationalMapper(Func<ISqliteConnection> connectionFactory, Func<TContext> contextFactory,
        Func<ISqliteOrmDatabaseContext, IEntityServices> entityServicesFactory)
    {
        this.connectionFactory = connectionFactory;
        this.contextFactory = contextFactory;
        this.entityServicesFactory = entityServicesFactory;
    }
    
    public string Filename { get; set; }

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

    private ISqliteConnection DatabaseConnection
    {
        get
        {
            if (_connection is null)
            {
                _connection = connectionFactory();
                _connection.OpenReadWrite(Filename, true);
            }

            return _connection;
        }
    }

    public virtual void Dispose()
    {
        if (_connection is not null)
        {
            if (_transaction is not null)
            {
                _transaction?.Dispose();
                _transaction = null;
            }
            
            _connection.Dispose();
            _connection = null;
        }
    }
    
    public ISqliteCommand CreateSqlCommand() => DatabaseConnection.CreateCommand();

    public void BeginTransaction()
    {
        if (_transaction is not null) throw new InvalidOperationException("The global transaction has already been started.");
        _transaction = DatabaseConnection.BeginTransaction();
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
            }
        }
    }    

    public int ExecuteNonQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        using (var command = CreateSqlCommand())
        {
            populateParamsAction?.Invoke(command.Parameters);
            return command.ExecuteNonQuery(sql);
        }
    }

    public ISqliteDataReader ExecuteQuery(string sql, Action<ISqliteParameterCollectionAddTo> populateParamsAction = null)
    {
        using (var command = CreateSqlCommand())
        {
            populateParamsAction?.Invoke(command.Parameters);
            return command.ExecuteQuery(sql);
        }
    }
    
    public bool Insert<T>(T entity)
    {
        return EntityServices.Insert(DatabaseConnection, entity);
    }
    
    public int InsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.InsertMany(DatabaseConnection, entities);
    }
    
    public bool Update<T>(T entity)
    {
        return EntityServices.Update(DatabaseConnection, entity); 
    }
    
    public int UpdateMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpdateMany(DatabaseConnection, entities);
    }
    
    public UpsertResult Upsert<T>(T entity)
    {
        return EntityServices.Upsert(DatabaseConnection, entity);
    }
    
    public UpsertManyResult UpsertMany<T>(IEnumerable<T> entities)
    {
        return EntityServices.UpsertMany(DatabaseConnection, entities);
    }

    public ISqliteQueryable<T> Get<T>(bool loadNavigationProps = false) where T : new()
    {
        return EntityServices.Get<T>(DatabaseConnection, loadNavigationProps);
    }
    
    public int Delete<T>(Expression<Func<T, bool>> predicate)
    {
        return EntityServices.Delete(DatabaseConnection, predicate);
    }

    public int DeleteAll<T>()
    {
        return EntityServices.DeleteAll<T>(DatabaseConnection);
    }
}