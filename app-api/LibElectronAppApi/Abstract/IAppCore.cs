using LibElectronAppApi.Models;

namespace LibElectronAppApi.Abstract;

public interface IAppCore
{
    event EventHandler<AppNotificationEventArgs> AppNotification;
    event EventHandler<TaskProgressEventArgs> TaskProgress;
    
    void InitCore();

    void DeInitCore();

    void AppNotify(int eventId, object eventData = null);
    
    void CancelInteractiveTask(Guid taskId);
}