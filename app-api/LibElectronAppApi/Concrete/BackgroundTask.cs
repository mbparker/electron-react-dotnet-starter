using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;
using LibElectronAppApi.Shared.Abstract;

namespace LibElectronAppApi.Concrete;

public class BackgroundTask : IBackgroundTask, IBackgroundTaskProgressHandler
{
    private readonly CancellationTokenSource cancelTokenSource;
    private readonly Action<IBackgroundTaskProgressHandler> taskAction;
    private readonly Func<IBackgroundTaskProgressHandler, Task> taskTask;
    private bool disposed = false;
    private bool didComplete = false;

    public BackgroundTask(string taskName, Action<IBackgroundTaskProgressHandler> taskAction)
    {
        this.taskAction = taskAction;
        cancelTokenSource = new CancellationTokenSource();
        TaskId = Guid.NewGuid();
        TaskName = taskName;
    }
    
    public BackgroundTask(string taskName, Func<IBackgroundTaskProgressHandler, Task> taskTask)
    {
        this.taskTask = taskTask;
        cancelTokenSource = new CancellationTokenSource();
        TaskId = Guid.NewGuid();
        TaskName = taskName;
    }
    
    public event EventHandler<TaskProgressEventArgs> Progress;

    public void Dispose()
    {
        if (!disposed)
        {
            cancelTokenSource.Dispose();
            disposed = true;
        }
    }

    public Guid TaskId { get; }
    
    public string TaskName { get; }

    CancellationToken IBackgroundTaskProgressHandler.CancelToken => cancelTokenSource.Token;
    
    public void Cancel()
    {
        if (!cancelTokenSource.IsCancellationRequested)
        {
            cancelTokenSource.Cancel();
        }
    }

    public IBackgroundTask Start()
    {
        if (taskTask is not null)
        {
            Task.Run(async () =>
            {
                try
                {
                    await taskTask.Invoke(this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    ReportInteractiveTaskCompleted();
                }
            }).ConfigureAwait(false);
        }
        else if (taskAction is not null)
        {
            Task.Run(() =>
            {
                try
                {
                    taskAction.Invoke(this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    ReportInteractiveTaskCompleted();
                }
            }).ConfigureAwait(false);   
        }
        
        return this;
    }
    
    void IBackgroundTaskProgressHandler.ReportInteractiveTaskProgress(string statusLine1, string statusLine2, long total, long completed)
    {
        if (didComplete) return;
        Progress?.Invoke(this,
            new TaskProgressEventArgs(true, TaskId, TaskName, statusLine1, statusLine2, total, completed));
    }

    private void ReportInteractiveTaskCompleted()
    {
        if (didComplete) return;
        didComplete = true;
        Progress?.Invoke(this,
            new TaskProgressEventArgs(false, TaskId, string.Empty, string.Empty, string.Empty, 1, 1));
    }
}