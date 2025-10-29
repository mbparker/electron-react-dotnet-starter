using System.Text;
using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Models;
using Newtonsoft.Json;

namespace LibElectronAppDemo.Concrete;

public class SeedDataExtractor : ISeedDataExtractor
{
    private readonly IResourceExtractor resourceExtractor;
    
    public SeedDataExtractor(IResourceExtractor resourceExtractor)
    {
        this.resourceExtractor = resourceExtractor;    
    }
    
    public Discography LoadDiscography()
    {
        var data = resourceExtractor.ExtractResource("Resources", "MusicManagerData", "discography.json");
        if (data.Length > 0)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<Discography>(json);
        }

        return null;
    }

    public AlbumArtwork LoadAlbumArtwork(int number)
    {
        var raw = resourceExtractor.ExtractResource("Resources", "MusicManagerData", "covers", number.ToString("D2"), "png");
        if (raw.Length > 0)
        {
            var dataUri = $"data:image/png;base64,{Convert.ToBase64String(raw)}";
            return new AlbumArtwork{ RawData = raw, DataUri = dataUri };
        }

        return null;
    }
}