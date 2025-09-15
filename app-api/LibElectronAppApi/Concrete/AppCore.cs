using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;

namespace LibElectronAppApi.Concrete;

public class AppCore : IAppCore
{
    private readonly IBackgroundTaskManager backgroundTaskManager;
    private bool uiClosing;
    private bool initialized;

    public AppCore(IBackgroundTaskManager backgroundTaskManager)
    {
        this.backgroundTaskManager = backgroundTaskManager;
        HookEvents();
    }

    public event EventHandler<AppNotificationEventArgs> AppNotification;
    public event EventHandler<TaskProgressEventArgs> TaskProgress;
    
    public void InitCore()
    {
        if (!initialized && !uiClosing)
        {
            // Do init stuff here
            initialized = true;
            AppNotify(1);
        }
    }

    public void DeInitCore()
    {
        if (!uiClosing)
        {
            uiClosing = true;
            if (initialized)
            {
                // Do deinit stuff here
                initialized = false;
            }
        }
    }
    
    public void AppNotify(int eventId, object eventData = null)
    {
        AppNotification?.Invoke(this, new AppNotificationEventArgs(eventId, eventData));
    }
    
    public void CancelInteractiveTask(Guid taskId)
    {
        backgroundTaskManager.GetById(taskId)?.Cancel();
    }
    
    private void HookEvents()
    {
        backgroundTaskManager.Progress += BackgroundTaskManagerOnProgress;
    }

    private void BackgroundTaskManagerOnProgress(object sender, TaskProgressEventArgs e)
    {
        TaskProgress?.Invoke(sender, e);
    }
}