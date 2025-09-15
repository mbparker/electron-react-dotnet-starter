using ElectronAppApiHost.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ElectronAppApiHost.Abstract;

public interface IApiAppCore : IDisposable
{
    IHubContext<CommunicationsHub> CommsHub { get; }

    void DeInitCore();
}