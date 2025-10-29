using Newtonsoft.Json;

namespace LibElectronAppDemo.Models;

public class Discography
{
    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("studio_albums")] 
    public StudioAlbum[] StudioAlbums { get; set; } = [];
    
    public class StudioAlbum
    {
        [JsonProperty("album")]
        public string Album { get; set; }
        
        [JsonProperty("release_date")]
        public DateOnly ReleaseDate {get; set;}
        
        [JsonProperty("tracks")] 
        public AlbumTrack[] Tracks { get; set; } = [];
        
        public class AlbumTrack
        {
            [JsonProperty("title")]
            public string Title { get; set; }
            
            [JsonProperty("duration_ms")]
            public long DurationMs { get; set; }            
        }
    }
}