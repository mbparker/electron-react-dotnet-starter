using ElectronAppApiHost.Models;
using LibElectronAppApi.Abstract;
using Microsoft.AspNetCore.SignalR;

namespace ElectronAppApiHost.Hubs;

public class CommunicationsHub : Hub
{
    private readonly IAppCore appCore;
    
    public CommunicationsHub(IAppCore appCore)
    {
        this.appCore = appCore;    
    }
    
    public void ClientReady()
    {
        // The first client to invoke this will cause the init to happen. Any further invokes are noops.
        appCore.InitCore();
    }
    
    public async Task<PingResponse> PingServer(PingRequest data)
    {
        var response = new PingResponse { Message = data.Message };
        await PingClient(Clients.Caller, response);
        return response;
    }

    public void CancelInteractiveTask(Guid taskId)
    {
        appCore.CancelInteractiveTask(taskId);
    }
    
    private async Task PingClient(IClientProxy client, PingResponse response)
    {
        await client.SendAsync(nameof(PingClient), response, CancellationToken.None);
    }    
}