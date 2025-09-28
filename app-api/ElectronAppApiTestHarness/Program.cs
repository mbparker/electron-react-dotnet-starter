//#define LOG_SQL
//#define LOG_WHERE_CLAUSE_VISITS

using System.Diagnostics;
using Autofac;
using ElectronAppApiTestHarness;
using LibElectronAppApi.Abstract;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract.Orm;

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
        ConsoleLogger.WriteLine(ConsoleColor.DarkGreen, args.Message);
#endif
    };
    
    ormTracer.WhereClauseBuilderVisit += (sender, args) =>
    {
#if(LOG_WHERE_CLAUSE_VISITS)
        ConsoleLogger.WriteLine(ConsoleColor.DarkMagenta, args.Message);
#endif
    };
    
    var orm = container.Resolve<ISqliteObjectRelationalMapping<DemoContext>>();

    //orm.DeleteDatabase();
    
    var dbCreated = orm.CreateDatabaseIfNotExists();

    if (!dbCreated)
    {
        try
        {
            if (orm.Migrate())
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
    
    TimeSpan ExecuteTimed(Action action)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        action?.Invoke();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }    

    if (dbCreated)
    {
        int totalRecordCount = 0;
        var creationTime = ExecuteTimed(() =>
        {
            orm.BeginTransaction();
            try
            {
                var random = new Random(Environment.TickCount);

                // Seed the demo data
                var entities = new List<DemoEntity>();
                for (var i = 1; i <= 100; i++)
                {
                    var isEven = i % 2 == 0;
                    var isSixth = i % 6 == 0;
                    var isEighth = i % 8 == 0;
                    var entity = new DemoEntity
                    {
                        EnumValue = isEven ? Entitykind.Kind1 : Entitykind.Kind2,
                        StringValue = $"This is string value {i}", BoolValue = isSixth, LongValue = long.MinValue,
                        ULongValue = isEighth ? null : ulong.MaxValue, GuidValue = Guid.NewGuid(),
                        DateTimeOffsetValue = DateTimeOffset.Now,
                        DateOnlyValue = DateOnly.FromDateTime(DateTime.Today), DateTimeValue = DateTime.Now,
                        DecimalValue = Decimal.MaxValue, DoubleValue = double.MinValue,
                        TimeOnlyValue = TimeOnly.FromDateTime(DateTime.Now),
                        TimeSpanValue = isEighth ? null : TimeSpan.FromTicks(Environment.TickCount),
                        BlobValue = GenerateRandomBlob()
                    };

                    byte[] GenerateRandomBlob()
                    {
                        var len = random.Next(0, 4096);
                        if (len == 0) return null;
                        var buff = new byte[len];
                        random.NextBytes(buff);
                        return buff;
                    }

                    entities.Add(entity);
                }

                totalRecordCount += orm.InsertMany(entities);

                var tags = new List<CustomTag>();
                for (var i = 1; i <= 25; i++)
                {
                    var tag = new CustomTag { TagValue = $"This is tag text {i}" };
                    tags.Add(tag);
                }

                totalRecordCount += orm.InsertMany(tags);

                var links = new List<CustomTagLink>();
                foreach (var entity in entities)
                {
                    var tagIds = new HashSet<long>();
                    var count = random.Next(0, 15);
                    for (var i = 0; i < count; i++)
                    {
                        var index = random.Next(0, tags.Count - 1);
                        if (tagIds.Add(index))
                        {
                            var link = new CustomTagLink { EntityId = entity.Id, TagId = tags[index].Id };
                            links.Add(link);
                        }
                    }
                }

                totalRecordCount += orm.InsertMany(links);

                orm.CommitTransaction();
            }
            catch (Exception ex)
            {
                ConsoleLogger.WriteLine(ConsoleColor.Red, ex.ToString());
                orm.RollbackTransaction();
                throw;
            }
        });
        
        ConsoleLogger.WriteLine(ConsoleColor.Green, $"Seeded {totalRecordCount} record in {creationTime.TotalSeconds} second(s)");
    }
    
    var dumpFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "test-sqlite-record-dump.txt");
    ConsoleLogger.WriteLine(ConsoleColor.Green, $"Creating dump file at: {dumpFilename}");

    var fileOps = container.Resolve<IFileOperations>();
    var elapsedOverall = ExecuteTimed(() =>
    {
        using (var stream = fileOps.CreateFileStream(dumpFilename, FileMode.Create))
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("--------------------------------------------------------------------------------");
                writer.WriteLine("DUMPING ALL CUSTOM TAG ENTITIES");
                writer.WriteLine("--------------------------------------------------------------------------------");
                sqlStatementCount = 0;
                CustomTag[] tagRecords = [];
                var elapsedStep = ExecuteTimed(() =>
                {
                    tagRecords = orm.Get<CustomTag>().AsEnumerable().ToArray();
                    foreach (var record in tagRecords)
                    {
                        writer.WriteLine(record.ToString());
                    }
                });
                writer.WriteLine("--------------------------------------------------------------------------------");
                ConsoleLogger.WriteLine(ConsoleColor.Green, $"Read {tagRecords.Length} {nameof(CustomTag)} record(s), executing {sqlStatementCount} SQL queries in {elapsedStep.TotalSeconds} second(s)");

                writer.WriteLine("--------------------------------------------------------------------------------");
                writer.WriteLine("DUMPING ALL (MAIN) ENTITIES - WITHOUT NAVIGATION PROPS");
                writer.WriteLine("--------------------------------------------------------------------------------");
                sqlStatementCount = 0;
                DemoEntity[] entityRecords = [];
                elapsedStep = ExecuteTimed(() =>
                {
                    entityRecords = orm.Get<DemoEntity>(includeDetails: false).AsEnumerable().ToArray();
                    foreach (var record in entityRecords)
                    {
                        writer.WriteLine(record.ToString());
                    }
                });
                writer.WriteLine("--------------------------------------------------------------------------------");
                ConsoleLogger.WriteLine(ConsoleColor.Green, $"Read {entityRecords.Length} {nameof(DemoEntity)} record(s) without navigation props, executing {sqlStatementCount} SQL queries in {elapsedStep.TotalSeconds} second(s)");

                writer.WriteLine("--------------------------------------------------------------------------------");
                writer.WriteLine("DUMPING ALL CUSTOM TAG LINK ENTITIES - WITHOUT NAVIGATION PROPS");
                writer.WriteLine("--------------------------------------------------------------------------------");
                sqlStatementCount = 0;
                CustomTagLink[] linkRecords = [];
                elapsedStep = ExecuteTimed(() =>
                {
                    linkRecords = orm.Get<CustomTagLink>(includeDetails: false).AsEnumerable().ToArray();
                    foreach (var record in linkRecords)
                    {
                        writer.WriteLine(record.ToString());
                    }
                });
                writer.WriteLine("--------------------------------------------------------------------------------");
                ConsoleLogger.WriteLine(ConsoleColor.Green, $"Read {linkRecords.Length} {nameof(CustomTagLink)} record(s) without navigation props, executing {sqlStatementCount} SQL queries in {elapsedStep.TotalSeconds} second(s)");   

                writer.WriteLine("--------------------------------------------------------------------------------");
                writer.WriteLine("DUMPING ALL (MAIN) ENTITIES - *WITH* NAVIGATION PROPS");
                writer.WriteLine("--------------------------------------------------------------------------------");
                sqlStatementCount = 0;
                entityRecords = [];
                elapsedStep = ExecuteTimed(() =>
                {
                    entityRecords = orm.Get<DemoEntity>(includeDetails: true).AsEnumerable().ToArray();
                    foreach (var record in entityRecords)
                    {
                        writer.WriteLine(record.ToString());
                    }
                });
                writer.WriteLine("--------------------------------------------------------------------------------");
                ConsoleLogger.WriteLine(ConsoleColor.Green, $"Read {entityRecords.Length} {nameof(DemoEntity)} record(s) with navigation props, executing {sqlStatementCount} SQL queries in {elapsedStep.TotalSeconds} second(s)");

                writer.WriteLine("--------------------------------------------------------------------------------");
                writer.WriteLine("DUMPING ALL CUSTOM TAG LINK ENTITIES - *WITH* NAVIGATION PROPS");
                writer.WriteLine("--------------------------------------------------------------------------------");
                sqlStatementCount = 0;
                linkRecords = [];
                elapsedStep = ExecuteTimed(() =>
                {
                    linkRecords = orm.Get<CustomTagLink>(includeDetails: true).AsEnumerable().ToArray();
                    foreach (var record in linkRecords)
                    {
                        writer.WriteLine(record.ToString());
                    }                    
                });
                writer.WriteLine("--------------------------------------------------------------------------------");
                ConsoleLogger.WriteLine(ConsoleColor.Green, $"Read {linkRecords.Length} {nameof(CustomTagLink)} record(s) with navigation props, executing {sqlStatementCount} SQL queries in {elapsedStep.TotalSeconds} second(s)");
            }
        }
    });

    ConsoleLogger.WriteLine(ConsoleColor.Green, $"Created dump file, executing {sqlStatementCountTotal} total SQL queries in {elapsedOverall.TotalSeconds} second(s)");
}