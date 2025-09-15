using LibSqlite3Orm.Abstract.Orm;

namespace ElectronAppApiTestHarness;

public class DemoEntity
{
    public long Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; }
    public ISqliteQueryable<DemoEntityDetailItem> Items { get; set; }
}

public class DemoEntityDetailItem
{
    public long Id { get; set; }
    public long DemoId { get; set; }
    public string NoteText { get; set; }
}