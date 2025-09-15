// See https://aka.ms/new-console-template for more information

using Autofac;
using ElectronAppApiTestHarness;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Models.Orm;

using (var container = ContainerRegistration.RegisterDependencies())
{
    var orm = container.Resolve<ISqliteObjectRelationalMapping<DemoContext>>();
    orm.Context.Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "test.sqlite");
    
    orm.DeleteDatabase();
    
    orm.CreateDatabase();

    if (orm.Migrate())
        Console.WriteLine("Migration performed");
    else
    {
        if (orm.DetectedSchemaChanges.ManualMigrationRequired)
            Console.WriteLine("Manual migration required");
        else
            Console.WriteLine("Migration not required");
    }
    
    //return;

    var entity1 = new DemoEntity { Enabled = true, Created = DateTimeOffset.Now, Description = "This is some descriptive text for item 1."};
    var entity2 = new DemoEntity { Enabled = false, Created = DateTimeOffset.Now, Description = "This is some descriptive text for item 2."};
    orm.InsertMany([entity1]);

    var detail1_1 = new DemoEntityDetailItem { DemoId = entity1.Id, NoteText = "Detail note 1 for 1" };
    var detail1_2 = new DemoEntityDetailItem { DemoId = entity1.Id, NoteText = "Detail note 2 for 1" };
    orm.InsertMany([detail1_1, detail1_2]);
    
    entity1.Description += "UPDATED";
    entity1.Enabled = !entity1.Enabled;
    entity2.Description += "UPDATED";
    entity2.Enabled = !entity2.Enabled;
    
    var ret = orm.UpsertMany([entity1, entity2]);
    Console.WriteLine($"Updated: {ret.UpdateCount}");
    Console.WriteLine($"Inserted: {ret.InsertCount}");
    Console.WriteLine($"Failed: {ret.FailedCount}");
    
    Console.WriteLine(entity1.Id);
    Console.WriteLine(entity2.Id);
    
    var detail2_1 = new DemoEntityDetailItem { DemoId = entity2.Id, NoteText = "Detail note 1 for 2" };
    var detail2_2 = new DemoEntityDetailItem { DemoId = entity2.Id, NoteText = "Detail note 2 for 2" };
    orm.InsertMany([detail2_1, detail2_2]);
    
    entity2.Description += "UPDATED again";
    entity2.Enabled = !entity2.Enabled;
    Console.WriteLine(orm.Upsert(entity2));

    foreach (var entity in orm.Get<DemoEntity>(includeDetails: true).AsEnumerable())
    {
        Console.WriteLine($"{entity.Id} - {entity.Created} - {entity.Description} - {entity.Items?.AsEnumerable().Count() ?? 0}");
    }

    var val1 = 0;
    var val2 = 2;
    var singleEntity = orm.Get<DemoEntity>(includeDetails: true).Where(x => x.Id > val1 && x.Id == val2).AsEnumerable().SingleOrDefault();
    if (singleEntity is not null)
        Console.WriteLine($"{singleEntity.Id} - {singleEntity.Description} - {singleEntity.Items?.AsEnumerable().Count() ?? 0}");
    else
        Console.WriteLine("Existing record is NULL!");

    singleEntity = orm.Get<DemoEntity>().Where(x => x.Id < 0).AsEnumerable().SingleOrDefault();
    if (singleEntity is null)
        Console.WriteLine("Non-existent is null, as expected");
    else
        Console.WriteLine("Non-existent is NOT null!");

    // Contrived way of saying Id == 0
    var deleteCount = orm.Delete<DemoEntity>(x => x.Id < 1 && x.Id >= 0);
    Console.WriteLine(deleteCount); // 0

    // Make sure the same parameter gets re-used since they hold the same value.
    deleteCount = orm.Delete<DemoEntity>(x => x.Id < 1 || x.Id < 1);
    Console.WriteLine(deleteCount); // 0

    deleteCount = orm.Delete<DemoEntity>(x => x.Id == 1);
    Console.WriteLine(deleteCount); // 1

    var schemaOrm = container.Resolve<ISqliteObjectRelationalMapping<SqliteOrmSchemaContext>>();
    schemaOrm.Context.Filename = orm.Context.Filename;
    var migrations = schemaOrm.Get<SchemaMigration>()
        .OrderByDescending(x => x.Timestamp)
        .Take(1);

    // SQL should not be generated or executed until start enumeration!
    var filtered = migrations.AsEnumerable().SingleOrDefault();

    if (filtered is null)
        Console.WriteLine("Filtered record is null");
    else
        Console.WriteLine($"{filtered.Id} - {filtered.Timestamp}");

    // Should hit DB again
    filtered = migrations.AsEnumerable().FirstOrDefault();

    if (filtered is null)
        Console.WriteLine("Filtered record is null");
    else
        Console.WriteLine($"{filtered.Id} - {filtered.Timestamp}");

    var cntStr = " again";
    var contRec = orm.Get<DemoEntity>(includeDetails: true)
        .Where(x => x.Description.EndsWith(cntStr) || x.Description.Contains("oh hai"))
        .OrderBy(x => x.Id).AsEnumerable()
        .FirstOrDefault();
    if (contRec is null)
        Console.WriteLine("Did not find record with the search criteria.");
    else
        Console.WriteLine($"{contRec.Id} - {contRec.Description} - {contRec.Items?.AsEnumerable().Count() ?? 0}");
}