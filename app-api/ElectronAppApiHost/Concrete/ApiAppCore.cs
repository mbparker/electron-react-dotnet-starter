using ElectronAppApiHost.Abstract;
using ElectronAppApiHost.Hubs;
using LibElectronAppApi.Abstract;
using LibElectronAppApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace ElectronAppApiHost.Concrete;

public class ApiAppCore : IApiAppCore
{
    private readonly IAppCore appCore;
    private bool disposed;
    
    public ApiAppCore(IHubContext<CommunicationsHub> commsHubContext, IAppCore appCore)
    {
        CommsHub = commsHubContext;
        this.appCore = appCore;
        HookEvents();
    }

    ~ApiAppCore()
    {
        Dispose(false);
    }
    
    public IHubContext<CommunicationsHub> CommsHub { get; private set; }
    
    public void DeInitCore()
    {
        appCore.DeInitCore();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            disposed = true;
            PerformDispose(disposing);
        }
    }
    
    private void PerformDispose(bool disposing)
    {
        if (disposing)
        {
            UnhookEvents();
            CommsHub = null;
        }  
    }

    private void HookEvents()
    {
        appCore.AppNotification += AppCoreOnAppNotification;
        appCore.TaskProgress += AppCoreOnTaskProgress;
    }

    private void UnhookEvents()
    {
        appCore.AppNotification -= AppCoreOnAppNotification;
        appCore.TaskProgress -= AppCoreOnTaskProgress;
    }
    
    private void AppCoreOnAppNotification(object sender, AppNotificationEventArgs e)
    {
        CommsHub.Clients.All.SendAsync(nameof(appCore.AppNotification), e).ConfigureAwait(false);
    }
    
    private void AppCoreOnTaskProgress(object sender, TaskProgressEventArgs e)
    {
        CommsHub.Clients.All.SendAsync(nameof(appCore.TaskProgress), e).ConfigureAwait(false);
    }
}