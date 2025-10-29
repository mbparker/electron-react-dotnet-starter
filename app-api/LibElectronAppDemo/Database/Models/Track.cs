using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace LibElectronAppDemo.Database.Models;

public class Track : NamedEntity
{
    public long? GenreId { get; set; }
    public long ArtistId { get; set; }
    public long AlbumId { get; set; }
    public float Rating { get; set; }
    public DateTimeOffset DateAdded { get; set; }
    public int? DiscNumber { get; set; }
    public int? TrackNumber { get; set; }
    public TimeSpan Duration { get; set; }
    public string Filename { get; set; }
    [NotMapped]
    public Lazy<Genre> Genre { get; set; }
    [NotMapped]
    public Lazy<Artist> Artist { get; set; }
    [NotMapped]
    public Lazy<Album> Album { get; set; }
}