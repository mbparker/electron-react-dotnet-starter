using LibElectronAppApi.Models;
using LibSqlite3Orm.Models.Orm.OData;

namespace LibElectronAppApi.Abstract;

public interface IAppCore
{
    event EventHandler<AppNotificationEventArgs> AppNotification;
    event EventHandler<TaskProgressEventArgs> TaskProgress;
    
    bool IsDbConnected { get; }
    
    bool EnableOrmTracing { get; set; }
    
    void InitCore();

    void DeInitCore();

    void AppNotify(int eventId, object eventData = null);
    
    void CancelInteractiveTask(Guid taskId);

    Guid ReCreateDemoDb();

    ODataQueryResult<T> GetData<T>(string odataQuery) where T : new();
}