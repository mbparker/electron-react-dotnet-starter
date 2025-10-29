using Autofac;
using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Concrete;
using LibElectronAppDemo.Database;

namespace LibElectronAppDemo;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DemoProvider>().As<IDemoProvider>().SingleInstance();
        builder.RegisterType<DatabaseSeeder>().As<IDatabaseSeeder>().SingleInstance();
        builder.RegisterType<ResourceExtractor>().As<IResourceExtractor>().SingleInstance();
        builder.RegisterType<SeedDataExtractor>().As<ISeedDataExtractor>().SingleInstance();
        builder.RegisterType<MusicManagerDbContext>().SingleInstance();
    }
}