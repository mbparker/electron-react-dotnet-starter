using LibElectronAppApi.Abstract;
using LibElectronAppDemo.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElectronAppApiHost.Controllers.Demo;

[ApiController]
[Route("api/demo/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly IAppCore appCore;
    
    public AlbumController(IAppCore appCore)
    {
        this.appCore = appCore;    
    }
    
    public IActionResult Get() 
    {
        return Ok(appCore.Orm.ODataQuery<Album>(Request.QueryString.ToODataQuery()));
    }
}