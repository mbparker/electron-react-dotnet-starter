using Newtonsoft.Json;

namespace LibElectronAppDemo.Database.Models;

public class NamedEntityWithImage : NamedEntity
{
    [JsonIgnore]
    public byte[] Image { get; set; }
    public string InlineImage { get; set; }
}