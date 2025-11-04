using LibElectronAppApi.Models;
using LibElectronAppDemo.Database;
using LibSqlite3Orm.Abstract.Orm;

namespace LibElectronAppApi.Abstract;

public interface IAppCore
{
    event EventHandler<AppNotificationEventArgs> AppNotification;
    event EventHandler<TaskProgressEventArgs> TaskProgress;
    
    bool IsDbConnected { get; }
    
    bool EnableOrmTracing { get; set; }
    
    ISqliteObjectRelationalMapper<MusicManagerDbContext> Orm { get; }
    
    void InitCore();

    void DeInitCore();

    void AppNotify(AppNotificationKind kind, object eventData = null);
    
    void CancelInteractiveTask(Guid taskId);

    Guid ReCreateDemoDb();
}