using System.ComponentModel.DataAnnotations.Schema;
using LibSqlite3Orm.Abstract.Orm;
using Newtonsoft.Json;

namespace LibElectronAppDemo.Database.Models;

public class Album : NamedEntityWithImage
{
    public long ArtistId { get; set; }
    public DateOnly ReleaseDate { get; set; }
    [NotMapped]
    public Lazy<Artist> Artist { get; set; }
    [JsonIgnore]
    [NotMapped]
    public Lazy<ISqliteQueryable<Track>> Tracks { get; set; }
}