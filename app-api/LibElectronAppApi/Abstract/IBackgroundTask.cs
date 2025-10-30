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