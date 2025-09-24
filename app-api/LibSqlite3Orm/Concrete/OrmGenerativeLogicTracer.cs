using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Models.Events;

namespace LibSqlite3Orm.Concrete;

public class OrmGenerativeLogicTracer : IOrmGenerativeLogicTracer
{
    public event EventHandler<GenerativeLogicTraceEventArgs> SqlStatementExecuting;
    public event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    
    public void NotifySqlStatementExecuting(string sqlStatement)
    {
        SqlStatementExecuting?.Invoke(this, new GenerativeLogicTraceEventArgs(sqlStatement));
    }

    public void NotifyWhereClauseBuilderVisit(string message)
    {
        WhereClauseBuilderVisit?.Invoke(this, new GenerativeLogicTraceEventArgs(message));
    }
}