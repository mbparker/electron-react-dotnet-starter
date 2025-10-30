using Autofac;
using LibElectronAppApi.Abstract;
using LibElectronAppApi.Concrete;

namespace LibElectronAppApi;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<LibSqlite3Orm.ContainerModule>();
        builder.RegisterModule<LibElectronAppDemo.ContainerModule>();
        builder.RegisterModule<LibElectronAppApi.Shared.ContainerModule>();
        builder.RegisterType<AppCore>().As<IAppCore>().SingleInstance();
        builder.RegisterType<BackgroundTaskManager>().As<IBackgroundTaskManager>().SingleInstance();
        builder.RegisterType<BackgroundTask>().As<IBackgroundTask>().InstancePerDependency().ExternallyOwned();
    }
}