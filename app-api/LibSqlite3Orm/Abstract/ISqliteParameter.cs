namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameter
{
    string Name { get; }
    int Index { get; }
    
    void UseSerializer(ISqliteFieldSerializer serializer);
    void UseSerializer(Type modelType);
    
    void Set(object value);
    string GetDebugValue();

    void Bind(IntPtr statement);
}