using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.Concrete;

public class SqliteCommand : ISqliteCommand
{
    private readonly Func<ISqliteConnection, ISqliteCommand, IntPtr, ISqliteDataReader> dbReaderFactory;
    private readonly ISqlStatementNotifier sqlNotifier;
    private ISqliteConnection connection;

    public SqliteCommand(ISqliteConnection connection, ISqliteParameterCollection parameters,
        Func<ISqliteConnection, ISqliteCommand, IntPtr, ISqliteDataReader> dbReaderFactory,
        ISqlStatementNotifier sqlNotifier)
    {
        this.connection = connection;
        this.connection.BeforeDispose += ConnectionOnBeforeDispose;
        this.dbReaderFactory = dbReaderFactory;
        this.sqlNotifier = sqlNotifier;
        Parameters = parameters;
    }
    
    private IntPtr ConnectionHandle => connection?.GetHandle() ?? IntPtr.Zero;
    
    public ISqliteParameterCollection Parameters { get; }

    public void Dispose()
    {
        ReleaseConnection();
    }

    public int ExecuteNonQuery(IEnumerable<string> sql)
    {
        return ExecuteNonQuery(string.Join("\n", sql));
    }

    public int ExecuteNonQuery(string sql)
    {
        var affectedRows = 0;
        var queries = sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (queries.Length > 1 && Parameters.Count > 0)
            throw new InvalidOperationException(
                "Cannot use parameters when executing multiple SQL commands.");
        foreach(var query in queries)
            affectedRows += ExecuteNonQuerySingleStatement(query);
        return affectedRows;
    }
    
    public ISqliteDataReader ExecuteQuery(IEnumerable<string> sql)
    {
        return ExecuteQuery(string.Join("\n", sql));
    }

    public ISqliteDataReader ExecuteQuery(string sql)
    {
        sqlNotifier.NotifySqlStatementExecuting(sql);
        var statement = SqliteExternals.Prepare2(ConnectionHandle, sql);
        if (Parameters.Count > 0)
            Parameters.BindAll(statement);        
        return dbReaderFactory(connection, this, statement);
    }

    private int ExecuteNonQuerySingleStatement(string sql)
    {
        try
        {
            sqlNotifier.NotifySqlStatementExecuting(sql);
            var statement = SqliteExternals.Prepare2(ConnectionHandle, sql);
            try
            {
                if (Parameters.Count > 0)
                    Parameters.BindAll(statement);

                SqliteResult ret;
                do
                {
                    ret = SqliteExternals.Step(statement);
                    if (ret != SqliteResult.OK && ret != SqliteResult.Done)
                        throw new SqliteException(ret, SqliteExternals.ExtendedErrCode(ConnectionHandle),
                            SqliteExternals.ErrorMsg(ConnectionHandle));
                } while (ret != SqliteResult.Done);
                
                return SqliteExternals.Changes(ConnectionHandle);
            }
            finally
            {
                SqliteExternals.Finalize(statement);
            }
        }
        finally
        {
            Parameters.Clear();
        }
    }
    
    private void ConnectionOnBeforeDispose(object sender, EventArgs e)
    {
        ReleaseConnection();
    }

    private void ReleaseConnection()
    {
        if (connection is not null)
        {
            connection.BeforeDispose -= ConnectionOnBeforeDispose;
            connection = null;
        }  
    }
}