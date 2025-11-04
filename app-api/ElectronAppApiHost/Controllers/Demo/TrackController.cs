using LibElectronAppApi.Abstract;
using LibElectronAppDemo.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElectronAppApiHost.Controllers.Demo;

[ApiController]
[Route("api/demo/[controller]")]
public class TrackController : ControllerBase
{
    private readonly IAppCore appCore;
    
    public TrackController(IAppCore appCore)
    {
        this.appCore = appCore;    
    }
    
    public IActionResult Get() 
    {
        return Ok(appCore.Orm.ODataQuery<Track>(Request.QueryString.Value));
    }
}