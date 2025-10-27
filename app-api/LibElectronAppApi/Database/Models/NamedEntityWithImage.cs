using Newtonsoft.Json;

namespace LibElectronAppApi.Database.Models;

public class NamedEntityWithImage : NamedEntity
{
    [JsonIgnore]
    public byte[] Image { get; set; }
    public string InlineImage { get; set; }
}