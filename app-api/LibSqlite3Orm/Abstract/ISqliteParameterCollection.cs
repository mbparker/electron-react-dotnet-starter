namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterCollection : IEnumerable<ISqliteParameter>
{
    int Count { get; }
    ISqliteParameter this[int index] { get; }
    ISqliteParameter this[string name] { get; }
    
    ISqliteParameter Add(string name, object value);
    ISqliteParameter Add(string name, object value, Type converterType);
    ISqliteParameter Add(string name, object value, ISqliteValueConverter converter);

    void BindAll(IntPtr statement);
    void Clear();
}