using LibSqlite3Orm.Models.Events;

namespace LibSqlite3Orm.Abstract;

public interface ISqlStatementNotifier
{
    event EventHandler<SqlStatementExecutingEventArgs> SqlStatementExecuting;
    
    void NotifySqlStatementExecuting(string sqlStatement);
}