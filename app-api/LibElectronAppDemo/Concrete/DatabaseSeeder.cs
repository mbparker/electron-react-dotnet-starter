using LibElectronAppDemo.Abstract;
using LibElectronAppDemo.Database;
using LibElectronAppDemo.Database.Models;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;

namespace LibElectronAppDemo.Concrete;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly Func<ISqliteObjectRelationalMapper<MusicManagerDbContext>> ormFactory;
    private readonly ISeedDataExtractor dataExtractor;
    
    public DatabaseSeeder(Func<ISqliteObjectRelationalMapper<MusicManagerDbContext>> ormFactory, ISeedDataExtractor dataExtractor)
    {
        this.ormFactory = ormFactory;
        this.dataExtractor = dataExtractor;    
    }
    
    public void SeedDatabase(ISqliteConnection connection)
    {
        using var orm = ormFactory();
        orm.UseConnection(connection);
        orm.BeginTransaction();
        try
        {
            var genres = new List<Genre>();
            genres.Add(new Genre { Name = "Unspecified"});
            genres.Add(new Genre { Name = "Heavy Metal"});
            orm.InsertMany(genres);
                        
            var artists = new List<Artist>();
            artists.Add(new Artist { Name = "Unknown"});
            artists.Add(new Artist { Name = "Various"});
                        
            var discography = dataExtractor.LoadDiscography();
            var artist = new Artist { Name = discography.Artist };
            artists.Add(artist);
            orm.InsertMany(artists);

            for (var i = 0; i < discography.StudioAlbums.Length; i++)
            {
                var a = discography.StudioAlbums[i];
                var art = dataExtractor.LoadAlbumArtwork(i + 1);

                var album = new Album
                {
                    Name = a.Album, ArtistId = artist.Id,
                    ReleaseDate = a.ReleaseDate,
                    Image = art?.RawData, InlineImage = art?.DataUri
                };
                orm.Insert(album);
                        
                var tracks = new List<Track>();
                for (var j = 0; j < a.Tracks.Length; j++)
                {
                    var t = a.Tracks[j];
                    var fakeTrackFilename = $"path/to/{discography.Artist}/{a.Album}/{t.Title}.mp3";
                    tracks.Add(new Track
                    {
                        Name = t.Title, GenreId = genres[1].Id, ArtistId = artist.Id,
                        AlbumId = album.Id, DiscNumber = 1, TrackNumber = j + 1, Rating = 4.2f,
                        Duration = TimeSpan.FromMilliseconds(t.DurationMs),
                        DateAdded = new DateTimeOffset(DateTime.Now),
                        Filename = fakeTrackFilename
                    });
                }
                orm.InsertMany(tracks);
            }
            
            orm.CommitTransaction();
        }
        catch(Exception)
        {
            orm.RollbackTransaction();
            throw;
        }
    }
}