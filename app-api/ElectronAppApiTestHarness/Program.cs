#define LOG_SQL
#define LOG_WHERE_CLAUSE_VISITS

using System.Diagnostics;
using Autofac;
using ElectronAppApiTestHarness;
using LibElectronAppApi.Abstract;
using LibElectronAppApi.Database;
using LibElectronAppApi.Database.Models;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;

try
{
    using (var container = ContainerRegistration.RegisterDependencies())
    {
        int sqlStatementCountTotal = 0;
        int sqlStatementCount = 0;

        var ormTracer = container.Resolve<IOrmGenerativeLogicTracer>();

        ormTracer.SqlStatementExecuting += (sender, args) =>
        {
            sqlStatementCountTotal++;
            sqlStatementCount++;
#if(LOG_SQL)
            ConsoleLogger.WriteLine(ConsoleColor.DarkGreen, args.Message.Value);
#endif
        };

        ormTracer.WhereClauseBuilderVisit += (sender, args) =>
        {
#if(LOG_WHERE_CLAUSE_VISITS)
            ConsoleLogger.WriteLine(ConsoleColor.DarkMagenta, args.Message.Value);
#endif
        };

        var dbFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "music-man.sqlite");
        
        using (var connection = container.Resolve<Func<ISqliteConnection>>().Invoke())
        {
            var fileOps = container.Resolve<IFileOperations>();
            
            bool dbCreated = false;

            using (var dbManager = container.Resolve<ISqliteObjectRelationalMapperDatabaseManager<MusicManagerDbContext>>())
            {
                dbManager.UseConnection(connection);

                var dbExists = fileOps.FileExists(dbFilename);
                connection.OpenReadWrite(dbFilename, false);
                //dbManager.DeleteDatabase();
                //dbExists = false;

                if (!dbExists)
                {
                    if (!connection.Connected)
                        connection.OpenReadWrite(dbFilename, false);
                    dbManager.CreateDatabase();
                    dbCreated = true;   
                }

                if (!dbCreated)
                {
                    try
                    {
                        if (dbManager.Migrate())
                            ConsoleLogger.WriteLine(ConsoleColor.Cyan, "Migration performed");
                        else
                            ConsoleLogger.WriteLine(ConsoleColor.Cyan, "Migration not required");
                    }
                    catch (Exception e)
                    {
                        ConsoleLogger.WriteLine(ConsoleColor.Red, e.ToString());
                        return;
                    }
                }
            }

            TimeSpan ExecuteTimed(Action action)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                action?.Invoke();
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }

            using var orm = container.Resolve<ISqliteObjectRelationalMapper<MusicManagerDbContext>>();
            orm.UseConnection(connection);

            if (dbCreated)
            {
                int totalRecordCount = 0;
                var creationTime = ExecuteTimed(() =>
                {
                    orm.BeginTransaction();
                    try
                    {
                        // Seed the default data
                        var genres = new List<Genre>();
                        genres.Add(new Genre { Name = "Unspecified"});
                        genres.Add(new Genre { Name = "Heavy Metal"});
                        orm.InsertMany(genres);
                        
                        var artists = new List<Artist>();
                        artists.Add(new Artist { Name = "Unknown"});
                        artists.Add(new Artist { Name = "Various"});
                        artists.Add(new Artist { Name = "Iron Maiden"});
                        orm.InsertMany(artists);
                        
                        var albums = new List<Album>();
                        albums.Add(new Album
                        {
                            Name = "Iron Maiden", ArtistId = artists[2].Id,
                            ReleaseDate = DateOnly.FromDateTime(new DateTime(1980, 4, 11))
                        });
                        orm.InsertMany(albums);
                        
                        var tracks = new List<Track>();
                        tracks.Add(new Track
                        {
                            Name = "Iron Maiden", GenreId = null, ArtistId = artists[2].Id, 
                            AlbumId = albums[0].Id, TrackNumber = 8, Rating = 4.2f,
                            Duration = TimeSpan.FromMinutes(3.717),
                            DateAdded = new DateTimeOffset(DateTime.Now),
                            Filename = "/path/to/iron maiden/iron maiden/iron maiden.mp3"
                        });
                        orm.InsertMany(tracks);

                        orm.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.WriteLine(ConsoleColor.Red, ex.ToString());
                        orm.RollbackTransaction();
                        throw;
                    }
                });

                ConsoleLogger.WriteLine(ConsoleColor.Green,
                    $"Seeded {totalRecordCount} record(s) in {creationTime.TotalSeconds} second(s)");
            }

            var recs = orm.Get<Track>(true).Where(x => x.Artist.Value.Name == "Iron Maiden").AsEnumerable();
            foreach (var rec in recs)
            {
                Console.WriteLine(rec.Filename);
                Console.WriteLine(rec.Genre.Value?.Name ?? "Not Set");
            }
        }
    }
}
finally
{
    ConsoleLogger.Dispose();
}