using ElectronAppApiHost.Models;
using LibElectronAppApi.Abstract;
using LibElectronAppDemo.Database.Models;
using LibSqlite3Orm.Models.Orm.OData;
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

    public bool IsDbConnected()
    {
        return appCore.IsDbConnected;
    }

    public Guid ReCreateDemoDb()
    {
        return appCore.ReCreateDemoDb();
    }
    
    public ODataQueryResult<Genre> GetGenres(string odataQuery)
    {
        return appCore.GetData<Genre>(odataQuery);
    }
    
    public ODataQueryResult<Artist> GetArtists(string odataQuery)
    {
        return appCore.GetData<Artist>(odataQuery);
    }
    
    public ODataQueryResult<Album> GetAlbums(string odataQuery)
    {
        return appCore.GetData<Album>(odataQuery);
    }      
    
    public ODataQueryResult<Track> GetTracks(string odataQuery)
    {
        return appCore.GetData<Track>(odataQuery);
    }      
    
    private async Task PingClient(IClientProxy client, PingResponse response)
    {
        await client.SendAsync(nameof(PingClient), response, CancellationToken.None);
    }    
}