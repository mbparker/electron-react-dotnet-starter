using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Types.Orm;

namespace ElectronAppApiTestHarness;

public class DemoContext : SqliteOrmDatabaseContext
{
    public DemoContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory)
        : base(schemaBuilderFactory)
    {
        Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "test.sqlite");
    }
    
    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        var demoEntity = builder.HasTable<DemoEntity>();
        demoEntity.WithPrimaryKey(x => x.Id).IsAutoIncrement().IsNotNull();
        demoEntity.WithColumn(x => x.Created).IsNotNull();
        demoEntity.WithColumn(x => x.Description).IsNotNull().UsingCollation();
        demoEntity.WithColumn(x => x.Enabled).IsNotNull();

        var demoEntityDetail = builder.HasTable<DemoEntityDetailItem>();
        demoEntityDetail.WithPrimaryKey(x => x.Id).IsAutoGuid().IsNotNull();
        demoEntityDetail.WithColumn(x => x.DemoId).IsNotNull();
        demoEntityDetail.WithColumn(x => x.NoteText).IsNotNull().UsingCollation();
        demoEntityDetail
            .WithForeignKey(x => x.DemoId)
            .References<DemoEntity>(x => x.Id)
            .OnDelete(SqliteForeignKeyAction.Cascade)
            // Meaning DemoEntityDetailItem.DemoEntity (a prop on this table) is a foriegn table record
            .HasNavigationProperty(x => x.DemoEntity)
            // Meaning DemoEntity.Items (foriegn table prop) is a collection of DemoEntityDetailItem (this table)
            .HasForeignNavigationCollectionProperty<DemoEntity>(x => x.Items);
        
        var demoEntityIndex = builder.HasIndex<DemoEntity>();
        demoEntityIndex.WithColumn(x => x.Description).UsingCollation().SortedAscending();
    }
}