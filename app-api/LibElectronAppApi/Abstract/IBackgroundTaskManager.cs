using LibElectronAppApi.Models;

namespace LibElectronAppApi.Abstract;

public interface IBackgroundTaskManager
{
    event EventHandler<TaskProgressEventArgs> Progress;
    
    IBackgroundTask Create(string taskName, Action<IBackgroundTaskProgressHandler> taskAction);
    
    IBackgroundTask Create(string taskName, Func<IBackgroundTaskProgressHandler, Task> taskTask);

    IBackgroundTask GetById(Guid taskId);
}