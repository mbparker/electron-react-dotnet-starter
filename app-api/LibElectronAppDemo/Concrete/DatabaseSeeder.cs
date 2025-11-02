using LibElectronAppApi.Shared.Abstract;
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
    
    public void SeedDatabase(IBackgroundTaskProgressHandler progressHandler, ISqliteConnection connection, ref long totalWork, long lastWorkCompleted)
    {
        long workCompleted = lastWorkCompleted;
        long workTotal = ++totalWork;
        
        void ReportProgress(string step)
        {
            progressHandler?.ReportInteractiveTaskProgress("Populating database...", step, workTotal, workCompleted++);   
        }
        
        ReportProgress("Extracting and reading seed data");
        var discography = dataExtractor.LoadDiscography();
        totalWork += discography.StudioAlbums.Length + discography.StudioAlbums.Sum(a => a.Tracks.Length) + 2 + 3 + 8; // +2 genres +3 artists +8 calls to ReportProgress - doesn't have to be perfectly accurate.
        workTotal = totalWork;

        ReportProgress("Create ORM instance and begin transaction");
        using var orm = ormFactory();
        orm.UseConnection(connection);
        orm.BeginTransaction();
        try
        {
            ReportProgress("Adding genres");
            var genres = new List<Genre>();
            genres.Add(new Genre { Name = "Unspecified"});
            genres.Add(new Genre { Name = "Heavy Metal"});
            orm.InsertMany(genres);
                        
            ReportProgress("Adding artists");
            var artists = new List<Artist>();
            artists.Add(new Artist { Name = "Unknown"});
            artists.Add(new Artist { Name = "Various"});
            
            var artist = new Artist { Name = discography.Artist };
            artists.Add(artist);
            orm.InsertMany(artists);

            ReportProgress($"Adding albums for {artist.Name}");
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
                        
                ReportProgress($"Adding tracks for {artist.Name} - {album.Name}");
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
            
            ReportProgress("Committing transaction");
            orm.CommitTransaction();
        }
        catch(Exception)
        {
            ReportProgress("FAILED - Rolling back transaction");
            orm.RollbackTransaction();
            throw;
        }
    }
}