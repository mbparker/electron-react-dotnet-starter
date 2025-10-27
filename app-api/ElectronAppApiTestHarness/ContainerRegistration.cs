using Autofac;

namespace ElectronAppApiTestHarness;

public static class ContainerRegistration
{
    public static IContainer RegisterDependencies()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<LibElectronAppApi.ContainerModule>();
        return builder.Build();
    }
}