using Autofac;

namespace ElectronAppApiTestHarness;

public static class ContainerRegistration
{
    public static IContainer RegisterDependencies()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<LibElectronAppApi.ContainerModule>();
        builder.RegisterModule<LibSqlite3Orm.ContainerModule>();
        builder.RegisterType<DemoContext>().SingleInstance();
        return builder.Build();
    }
}