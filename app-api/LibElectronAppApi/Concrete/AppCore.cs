using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;
using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Database;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm.Events;
using LibSqlite3Orm.Models.Orm.OData;

namespace LibElectronAppApi.Concrete;

public class AppCore : IAppCore
{
    private readonly IBackgroundTaskManager backgroundTaskManager;
    private readonly IDemoProvider demoProvider;
    private readonly Func<ISqliteObjectRelationalMapper<MusicManagerDbContext>> ormFactory;
    private readonly IOrmGenerativeLogicTracer logicTracer;
    private ISqliteConnection dbConnection;
    private ISqliteObjectRelationalMapper<MusicManagerDbContext> orm;
    private bool uiClosing;
    private bool initialized;

    public AppCore(IBackgroundTaskManager backgroundTaskManager, IDemoProvider demoProvider,
        Func<ISqliteObjectRelationalMapper<MusicManagerDbContext>> ormFactory,
        IOrmGenerativeLogicTracer logicTracer)
    {
        this.backgroundTaskManager = backgroundTaskManager;
        this.demoProvider = demoProvider;
        this.ormFactory = ormFactory;
        this.logicTracer =  logicTracer;
        HookEvents();
    }

    public event EventHandler<AppNotificationEventArgs> AppNotification;
    public event EventHandler<TaskProgressEventArgs> TaskProgress;

    public bool IsDbConnected => dbConnection?.Connected ?? false;
    
    public void InitCore()
    {
        if (!initialized && !uiClosing)
        {
            // Do init stuff here
            dbConnection = demoProvider.TryConnectToDemoDb();
            if (dbConnection is not null)
            {
                orm = ormFactory();
                orm.UseConnection(dbConnection);
            }

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
                dbConnection?.Dispose();
                dbConnection = null;
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

    public Guid ReCreateDemoDb()
    {
        return backgroundTaskManager.Create("Recreate Database",
            ph =>
            {
                dbConnection?.Dispose();
                demoProvider.CreateDemoDb(ph, dropExisting: true);
                dbConnection =  demoProvider.TryConnectToDemoDb();
            }).Start().TaskId;
    }
    
    public Guid CreateDemoDbIfNeeded()
    {
        return backgroundTaskManager.Create("Ensure Database Created",
            ph =>
            {
                dbConnection?.Dispose();
                demoProvider.CreateDemoDb(ph);
                dbConnection =  demoProvider.TryConnectToDemoDb();
            }).Start().TaskId;
    }

    public ODataQueryResult<T> GetData<T>(string odataQuery) where T : new()
    {
        return IsDbConnected ? orm.ODataQuery<T>(odataQuery) : new ODataQueryResult<T>([], 0);
    }
    
    private void HookEvents()
    {
        backgroundTaskManager.Progress += BackgroundTaskManagerOnProgress;
        logicTracer.SqlStatementExecuting += LogicTracerOnSqlStatementExecuting;
    }

    private void LogicTracerOnSqlStatementExecuting(object sender, SqlStatementExecutingEventArgs e)
    {
        ConsoleLogger.WriteLine(e.Message.Value);
    }

    private void BackgroundTaskManagerOnProgress(object sender, TaskProgressEventArgs e)
    {
        TaskProgress?.Invoke(sender, e);
    }
}