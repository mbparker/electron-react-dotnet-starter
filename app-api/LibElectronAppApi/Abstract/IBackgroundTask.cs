using LibElectronAppApi.Models;

namespace LibElectronAppApi.Abstract;

public interface IBackgroundTask : IDisposable
{
    event EventHandler<TaskProgressEventArgs> Progress;

    Guid TaskId { get; }
    
    string TaskName { get; }

    void Cancel();

    IBackgroundTask Start();
}

public interface IBackgroundTaskProgressHandler
{
    CancellationToken CancelToken { get; }
    
    void ReportInteractiveTaskProgress(string statusLine1, string statusLine2, long total, long completed);
}