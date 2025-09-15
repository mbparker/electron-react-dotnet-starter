namespace LibSqlite3Orm.Abstract;

public interface ISqliteDataReader : IEnumerable<ISqliteDataRow>, IDisposable
{
    ISqliteConnection Connection { get; }
}