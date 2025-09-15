using System.Collections;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataRow : ISqliteDataRow
{
    private readonly IntPtr statement;
    private readonly Func<int, IntPtr, ISqliteDataColumn> columnFactory;
    private readonly List<ISqliteDataColumn> columns = new();
    
    public SqliteDataRow(IntPtr statement, Func<int, IntPtr, ISqliteDataColumn> columnFactory)
    {
        this.statement = statement;
        this.columnFactory = columnFactory;
        ColumnCount = SqliteExternals.ColumnCount(statement);
        PopulateColumns();
    }

    public int ColumnCount { get; }

    public ISqliteDataColumn this[int index] => columns[index];

    public ISqliteDataColumn this[string name] =>
        columns.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    
    public IEnumerator<ISqliteDataColumn> GetEnumerator()
    {
        return columns.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }    
    
    private void PopulateColumns()
    {
        if (ColumnCount == 0) return;
        for (var i = 0; i < ColumnCount; i++)
        {
            columns.Add(columnFactory.Invoke(i, statement));
        }
    }    
}