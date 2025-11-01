#define LOG_SQL
#define LOG_WHERE_CLAUSE_VISITS

using Autofac;
using ElectronAppApiTestHarness;
using LibElectronAppApi.Shared;
using LibElectronAppApi.Shared.Abstract;
using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Database;
using LibElectronAppDemo.Database.Models;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract.Orm;

try
{
    using var container = ContainerRegistration.RegisterDependencies();
    var fileOps = container.Resolve<IFileOperations>();
    ConsoleLogger.Filename = Path.Combine(fileOps.GetLocalAppDataPathForCurrentPlatform(), SharedConstants.AppName,
        "Logs", $"TestHarnessConsoleLog-{DateTime.Today.Year}-{DateTime.Today.Month}-{DateTime.Today.Day}.log");
    var ormTracer = container.Resolve<IOrmGenerativeLogicTracer>();

    ormTracer.SqlStatementExecuting += (sender, args) =>
    {
#if(LOG_SQL)
        ConsoleLogger.WriteLine(ConsoleColor.DarkGreen, args.Message.Value);
#endif
    };

    ormTracer.WhereClauseBuilderVisit += (sender, args) =>
    {
#if(LOG_WHERE_CLAUSE_VISITS)
        ConsoleLogger.WriteLine(ConsoleColor.DarkMagenta, args.Message.Value);
#endif
    };
        
    var demoProvider = container.Resolve<IDemoProvider>();
    demoProvider.CreateDemoDb(progressHandler: null);
    var connection = demoProvider.TryConnectToDemoDb();
    if (connection is not null)
    {
        using (connection)
        {
            using var orm = container.Resolve<ISqliteObjectRelationalMapper<MusicManagerDbContext>>();
            orm.UseConnection(connection);

            var recs = orm.Get<Track>(true).AsEnumerable();
            foreach (var rec in recs)
            {
                Console.WriteLine(rec.Filename);
            }            
        }
    }
}
finally
{
    ConsoleLogger.Filename = null;
}