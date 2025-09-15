using Newtonsoft.Json;

namespace ElectronAppApiHost.Models;

public class PingResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }
}