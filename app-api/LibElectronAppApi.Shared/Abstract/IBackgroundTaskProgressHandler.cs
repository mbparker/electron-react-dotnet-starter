namespace LibElectronAppApi.Shared.Abstract;

public interface IBackgroundTaskProgressHandler
{
    CancellationToken CancelToken { get; }
    
    void ReportInteractiveTaskProgress(string statusLine1, string statusLine2, long total, long completed);
}