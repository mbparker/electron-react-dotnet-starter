using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Models.Events;

namespace LibSqlite3Orm.Concrete;

public class SqlStatementNotifier : ISqlStatementNotifier
{
    public event EventHandler<SqlStatementExecutingEventArgs> SqlStatementExecuting;
    
    public void NotifySqlStatementExecuting(string sqlStatement)
    {
        SqlStatementExecuting?.Invoke(this, new SqlStatementExecutingEventArgs(sqlStatement));
    }
}