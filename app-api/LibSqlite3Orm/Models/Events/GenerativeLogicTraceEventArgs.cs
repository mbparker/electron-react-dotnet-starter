namespace LibSqlite3Orm.Models.Events;

public class GenerativeLogicTraceEventArgs : EventArgs
{
    public GenerativeLogicTraceEventArgs(string message)
    {
        Message = message;
    }
    
    public string Message { get; }
}