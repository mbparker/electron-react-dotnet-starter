namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameter
{
    string Name { get; }
    int Index { get; }
    
    void UseConverter(ISqliteValueConverter converter);
    void UseConverter(Type converterType);
    
    void Set(object value);

    void Bind(IntPtr statement);
}