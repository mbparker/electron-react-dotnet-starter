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
        demoEntity.WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        demoEntity.WithColumnChanges(x => x.StringValue).UsingCollation();
        
        var customTag = builder.HasTable<CustomTag>();
        customTag.WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        customTag.WithColumnChanges(x => x.TagValue).UsingCollation().IsUnique().IsNotNull();
        
        var customTagLink = builder.HasTable<CustomTagLink>();
        customTagLink.WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        customTagLink
            .WithForeignKey(x => x.TagId)
            .References<CustomTag>(x => x.Id)
            .HasNavigationProperty(x => x.Tag)
            .OnDelete(SqliteForeignKeyAction.Cascade);
        customTagLink
            .WithForeignKey(x => x.EntityId)
            .References<DemoEntity>(x => x.Id)
            .HasNavigationProperty(x => x.Entity)
            .HasForeignNavigationProperty<DemoEntity>(x => x.Tags)
            .OnDelete(SqliteForeignKeyAction.Cascade);
        
        builder.HasIndex<DemoEntity>().WithColumn(x => x.StringValue).UsingCollation().SortedAscending();
        builder.HasIndex<CustomTag>().WithColumn(x => x.TagValue).UsingCollation().SortedAscending();
        builder.HasIndex<CustomTagLink>().WithColumn(x => x.EntityId).UsingCollation().SortedAscending();
        builder.HasIndex<CustomTagLink>().WithColumn(x => x.TagId).UsingCollation().SortedAscending();
    }
}