using Autofac;
using Autofac.Extensions.DependencyInjection;
using LibElectronAppApi;
using ElectronAppApiHost.Abstract;
using ElectronAppApiHost.Concrete;
using ElectronAppApiHost.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add modules and services to the container.
builder.Host.ConfigureContainer<ContainerBuilder>(contBldr =>
{
    contBldr.RegisterModule<ContainerModule>();
    contBldr.RegisterType<ApiAppCore>().As<IApiAppCore>().SingleInstance();
});

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSignalR(opt =>
{
    opt.EnableDetailedErrors = true;
    opt.MaximumReceiveMessageSize = 65536;
    opt.DisableImplicitFromServicesParameters = true;
}).AddNewtonsoftJsonProtocol(opt =>
{
    opt.PayloadSerializerSettings.Error += (sender, eventArgs) =>
    {
        Console.WriteLine(eventArgs.ErrorContext.Error);
    };
});

var app = builder.Build();

app.MapControllers();
app.MapHub<CommunicationsHub>("/comms", opt =>
{
});

var apiAppCore = app.Services.GetRequiredService<IApiAppCore>();
try
{
    app.Run();
    Environment.ExitCode = 0;
}
finally
{
    // First client to connect will initialize it
    apiAppCore.DeInitCore();
}