using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;
using LibElectronAppApi.Shared.Abstract;

namespace LibElectronAppApi.Concrete;

public class BackgroundTaskManager : IBackgroundTaskManager
{
    private readonly object syncObj = new();
    private readonly Func<string, Action<IBackgroundTaskProgressHandler>, IBackgroundTask> backgroundTaskFactory1;
    private readonly Func<string, Func<IBackgroundTaskProgressHandler, Task>, IBackgroundTask> backgroundTaskFactory2;
    private readonly Dictionary<Guid, IBackgroundTask> tasks;

    public BackgroundTaskManager(Func<string, Action<IBackgroundTaskProgressHandler>, IBackgroundTask> backgroundTaskFactory1,
        Func<string, Func<IBackgroundTaskProgressHandler, Task>, IBackgroundTask> backgroundTaskFactory2)
    {
        this.backgroundTaskFactory1 = backgroundTaskFactory1;
        this.backgroundTaskFactory2 = backgroundTaskFactory2;
        tasks = new Dictionary<Guid, IBackgroundTask>();
    }
    
    public event EventHandler<TaskProgressEventArgs> Progress;
    
    public IBackgroundTask Create(string taskName, Action<IBackgroundTaskProgressHandler> taskAction)
    {
        var task = backgroundTaskFactory1(taskName, taskAction);
        lock (syncObj)
        {
            tasks.Add(task.TaskId, task);
        }

        task.Progress += HandleProgress;
        
        return task;
    }

    public IBackgroundTask Create(string taskName, Func<IBackgroundTaskProgressHandler, Task> taskTask)
    {
        var task = backgroundTaskFactory2(taskName, taskTask);
        lock (syncObj)
        {
            tasks.Add(task.TaskId, task);
        }

        task.Progress += HandleProgress;
        
        return task;
    }

    public IBackgroundTask GetById(Guid taskId)
    {
        lock (syncObj)
        {
            if (tasks.TryGetValue(taskId, out IBackgroundTask task))
            {
                return task;
            }

            return null;
        }
    }

    private void HandleProgress(object sender, TaskProgressEventArgs args)
    {
        Progress?.Invoke(sender, args);
        if (args.ShowDialog) return;
        if (sender is not IBackgroundTask task) return;
        lock (syncObj)
        {
            tasks.Remove(task.TaskId);
        }

        task.Progress -= HandleProgress;
        task.Dispose();
    }
}