using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.Abstract.Orm;

public interface IOrmGenerativeLogicTracer
{
    event EventHandler<SqlStatementExecutingEventArgs> SqlStatementExecuting;
    event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    
    void NotifySqlStatementExecuting(string sqlStatement, ISqliteParameterCollection parameters);
    void NotifyWhereClauseBuilderVisit(Lazy<string> message);
}