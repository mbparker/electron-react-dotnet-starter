using Autofac;
using LibElectronAppApi.Shared.Abstract;
using LibElectronAppApi.Shared.Concrete;

namespace LibElectronAppApi.Shared;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<GuidGenerator>().As<IGuidGenerator>().SingleInstance();
        builder.RegisterType<FileOperations>().As<IFileOperations>().SingleInstance();
        builder.RegisterType<WebClientWrapper>().As<IWebClient>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<ImageResizer>().As<IImageResizer>().SingleInstance();        
    }
}