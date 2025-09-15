namespace LibElectronAppApi.Models;

public class TaskProgressEventArgs : EventArgs
{
    public TaskProgressEventArgs(bool showDialog, Guid taskId, string title, string statusLine1, string statusLine2, long total, long completed)
    {
        ShowDialog = showDialog;
        TaskId = taskId;
        Title = title;
        StatusLine1 = statusLine1;
        StatusLine2 = statusLine2;
        Total = total;
        Completed = completed;
    }
    
    public bool ShowDialog { get; }
    
    public Guid TaskId { get; }
    
    public string Title { get; }
    
    public string StatusLine1 { get; }
    
    public string StatusLine2 { get; }
    
    public long Total { get; }
    
    public long Completed { get; }
}