using Autofac;
using LibElectronAppApi.Abstract;
using LibElectronAppApi.Concrete;
using LibElectronAppApi.Database;

namespace LibElectronAppApi;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<LibSqlite3Orm.ContainerModule>();
        builder.RegisterType<AppCore>().As<IAppCore>().SingleInstance();
        builder.RegisterType<GuidGenerator>().As<IGuidGenerator>().SingleInstance();
        builder.RegisterType<FileOperations>().As<IFileOperations>().SingleInstance();
        builder.RegisterType<WebClientWrapper>().As<IWebClient>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<BackgroundTaskManager>().As<IBackgroundTaskManager>().SingleInstance();
        builder.RegisterType<BackgroundTask>().As<IBackgroundTask>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<ImageResizer>().As<IImageResizer>().SingleInstance();
        builder.RegisterType<MusicManagerDbContext>().SingleInstance();
    }
}