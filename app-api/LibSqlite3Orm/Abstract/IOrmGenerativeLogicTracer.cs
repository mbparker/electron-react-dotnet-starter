using LibSqlite3Orm.Models.Events;

namespace LibSqlite3Orm.Abstract;

public interface IOrmGenerativeLogicTracer
{
    event EventHandler<GenerativeLogicTraceEventArgs> SqlStatementExecuting;
    event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    
    void NotifySqlStatementExecuting(string sqlStatement);
    void NotifyWhereClauseBuilderVisit(string message);
}