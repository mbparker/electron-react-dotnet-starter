using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;
using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Database;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm.Events;

namespace LibElectronAppApi.Concrete;

public class AppCore : IAppCore
{
    private readonly IBackgroundTaskManager backgroundTaskManager;
    private readonly IDemoProvider demoProvider;
    private readonly Func<ISqliteObjectRelationalMapper<MusicManagerDbContext>> ormFactory;
    private readonly IOrmGenerativeLogicTracer logicTracer;
    private ISqliteConnection dbConnection;
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
    
    public bool EnableOrmTracing { get; set; }
    
    public ISqliteObjectRelationalMapper<MusicManagerDbContext> Orm { get; private set; }
    
    public void InitCore()
    {
        if (!initialized && !uiClosing)
        {
            // Do init stuff here
            CreateDemoDbIfNeeded();
            TryConnectToDemoDb();
            initialized = true;
            AppNotify(AppNotificationKind.ApiInitialized);
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
    
    public void AppNotify(AppNotificationKind kind, object eventData = null)
    {
        AppNotification?.Invoke(this, new AppNotificationEventArgs(kind, eventData));
    }
    
    public void CancelBackgroundTask(Guid taskId)
    {
        backgroundTaskManager.GetById(taskId)?.Cancel();
    }

    public Guid StartReCreateDemoDbTask()
    {
        return backgroundTaskManager.Create("Recreate Database",
            ph =>
            {
                dbConnection?.Dispose();
                demoProvider.CreateDemoDb(ph, dropExisting: true);
                TryConnectToDemoDb();              
            }).Start().TaskId;
    }
    
    private void CreateDemoDbIfNeeded()
    {
        backgroundTaskManager.Create("Create Database",
            ph =>
            {
                dbConnection?.Dispose();
                demoProvider.CreateDemoDb(ph);
                TryConnectToDemoDb();
            }).Start();
    }    
    
    private void TryConnectToDemoDb()
    {
        dbConnection =  demoProvider.TryConnectToDemoDb();
        if (dbConnection is not null)
        {
            Orm?.Dispose();
            Orm = ormFactory();
            Orm.UseConnection(dbConnection);
            AppNotify(AppNotificationKind.DatabaseConnected);
        }         
    }    
    
    private void HookEvents()
    {
        backgroundTaskManager.Progress += BackgroundTaskManagerOnProgress;
        logicTracer.SqlStatementExecuting += LogicTracerOnSqlStatementExecuting;
        logicTracer.WhereClauseBuilderVisit += LogicTracerOnWhereClauseBuilderVisit;
    }

    private void LogicTracerOnWhereClauseBuilderVisit(object sender, GenerativeLogicTraceEventArgs e)
    {
        if (EnableOrmTracing)
            ConsoleLogger.WriteLine(e.Message.Value);
    }

    private void LogicTracerOnSqlStatementExecuting(object sender, SqlStatementExecutingEventArgs e)
    {
        if (EnableOrmTracing)
            ConsoleLogger.WriteLine(e.Message.Value);
    }

    private void BackgroundTaskManagerOnProgress(object sender, TaskProgressEventArgs e)
    {
        TaskProgress?.Invoke(sender, e);
    }
}