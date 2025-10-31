using System.ComponentModel.DataAnnotations.Schema;
using LibSqlite3Orm.Abstract.Orm;
using Newtonsoft.Json;

namespace LibElectronAppDemo.Database.Models;

public class Artist : NamedEntityWithImage
{
    [JsonIgnore]
    [NotMapped]
    public Lazy<ISqliteQueryable<Album>> Albums { get; set; }
}