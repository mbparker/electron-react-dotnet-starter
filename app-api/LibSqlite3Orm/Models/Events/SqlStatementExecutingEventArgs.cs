namespace LibSqlite3Orm.Models.Events;

public class SqlStatementExecutingEventArgs : EventArgs
{
    public SqlStatementExecutingEventArgs(string sqlStatement)
    {
        SqlStatement = sqlStatement;
    }
    
    public string SqlStatement { get; }
}