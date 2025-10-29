using System.ComponentModel.DataAnnotations.Schema;
using LibSqlite3Orm.Abstract.Orm;

namespace LibElectronAppDemo.Database.Models;

public class Artist : NamedEntityWithImage
{
    [NotMapped]
    public Lazy<ISqliteQueryable<Album>> Albums { get; set; }
}