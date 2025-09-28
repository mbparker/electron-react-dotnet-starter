namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterDebug
{
    string Name { get; }
    int Index { get; }    
    string GetDebugValue();
}

public interface ISqliteParameter
{
    string Name { get; }
    int Index { get; }
    
    void UseSerializer(ISqliteFieldSerializer serializer);
    void UseSerializer(Type modelType);
    
    void Set(object value);

    void Bind(IntPtr statement);
}