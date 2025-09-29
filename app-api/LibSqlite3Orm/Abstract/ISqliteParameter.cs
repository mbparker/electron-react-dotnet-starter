using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterDebug
{
    string Name { get; }
    int Index { get; }
    object DeserializedValue { get; }
    object SerialzedValue { get; }
    SqliteColType SerializedTypeAffinity { get; }    
    string GetDebugValue();
}

public interface ISqliteParameter
{
    string Name { get; }
    int Index { get; }
    object DeserializedValue { get; }
    object SerialzedValue { get; }
    SqliteColType SerializedTypeAffinity { get; }
    void Set(object value);
    void Bind(IntPtr statement);
}