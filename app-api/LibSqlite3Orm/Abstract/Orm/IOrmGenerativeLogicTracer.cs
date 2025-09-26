using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.Abstract.Orm;

public interface IOrmGenerativeLogicTracer
{
    event EventHandler<GenerativeLogicTraceEventArgs> SqlStatementExecuting;
    event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    
    void NotifySqlStatementExecuting(string sqlStatement);
    void NotifyWhereClauseBuilderVisit(string message);
}