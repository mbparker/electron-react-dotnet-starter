using LibElectronAppDemo.Database.Models;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibElectronAppDemo.Database;

public class MusicManagerDbContext : SqliteOrmDatabaseContext
{
    public MusicManagerDbContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory) 
        : base(schemaBuilderFactory)
    {
    }

    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        var genreTable = builder.HasTable<Genre>();
        genreTable.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        genreTable.WithColumn(x => x.Name).IsUnique().IsNotNull().UsingCollation();
        
        var artistTable = builder.HasTable<Artist>();
        artistTable.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        artistTable.WithColumn(x => x.Name).IsUnique().IsNotNull().UsingCollation();
        artistTable.WithColumn(x => x.Image);
        artistTable.WithColumn(x => x.InlineImage);
        
        var albumTable = builder.HasTable<Album>();
        albumTable.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        albumTable.WithColumn(x => x.Name).IsNotNull().UsingCollation();
        albumTable.WithColumn(x => x.ArtistId).IsNotNull();
        albumTable.WithColumn(x => x.ReleaseDate).IsNotNull();
        albumTable.WithColumn(x => x.Image);
        albumTable.WithColumn(x => x.InlineImage);
        albumTable.WithForeignKey(x => x.ArtistId)
            .References<Artist>(x => x.Id)
            .HasOne(x => x.Artist)
            .WithMany<Artist>(x => x.Albums)
            .OnDelete(SqliteForeignKeyAction.Cascade)
            .OnUpdate(SqliteForeignKeyAction.Cascade);
        
        var trackTable = builder.HasTable<Track>();
        trackTable.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        trackTable.WithColumn(x => x.Name).IsNotNull().UsingCollation();
        trackTable.WithColumn(x => x.GenreId);
        trackTable.WithColumn(x => x.ArtistId).IsNotNull();
        trackTable.WithColumn(x => x.AlbumId).IsNotNull();
        trackTable.WithColumn(x => x.DateAdded).IsNotNull();
        trackTable.WithColumn(x => x.DiscNumber);
        trackTable.WithColumn(x => x.TrackNumber);
        trackTable.WithColumn(x => x.Rating).IsNotNull().WithDefaultValue(0.0f);
        trackTable.WithColumn(x => x.Duration);
        trackTable.WithColumn(x => x.Filename).IsUnique().IsNotNull();
        trackTable.WithForeignKey(x => x.GenreId)
            .References<Genre>(x => x.Id)
            .HasOne(x => x.Genre)
            .OnDelete(SqliteForeignKeyAction.SetNull)
            .OnUpdate(SqliteForeignKeyAction.Cascade)
            .IsOptional();
        trackTable.WithForeignKey(x => x.ArtistId)
            .References<Artist>(x => x.Id)
            .HasOne(x => x.Artist)
            .OnDelete(SqliteForeignKeyAction.NoAction)
            .OnUpdate(SqliteForeignKeyAction.Cascade);
        trackTable.WithForeignKey(x => x.AlbumId)
            .References<Album>(x => x.Id)
            .HasOne(x => x.Album)
            .WithMany<Album>(x => x.Tracks)
            .OnDelete(SqliteForeignKeyAction.Cascade)
            .OnUpdate(SqliteForeignKeyAction.Cascade);
    }
}