using Newtonsoft.Json;

namespace ElectronAppApiHost.Models;

public class PingRequest
{
    [JsonProperty("message")]
    public string Message { get; set; }
}