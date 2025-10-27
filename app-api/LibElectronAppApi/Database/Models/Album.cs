using System.ComponentModel.DataAnnotations.Schema;
using LibSqlite3Orm.Abstract.Orm;

namespace LibElectronAppApi.Database.Models;

public class Album : NamedEntityWithImage
{
    public long ArtistId { get; set; }
    public DateOnly ReleaseDate { get; set; }
    [NotMapped]
    public Lazy<Artist> Artist { get; set; }
    [NotMapped]
    public Lazy<ISqliteQueryable<Track>> Tracks { get; set; }
}