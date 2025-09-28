namespace LibSqlite3Orm.Abstract;

public interface ISqliteParameterCollectionAddTo
{
    ISqliteParameter Add(string name, object value);
    ISqliteParameter Add(string name, object value, Type modelType);
    ISqliteParameter Add(string name, object value, ISqliteFieldSerializer serializer);
}

public interface ISqliteParameterCollection : IEnumerable<ISqliteParameter>, ISqliteParameterCollectionAddTo
{
    int Count { get; }
    ISqliteParameter this[int index] { get; }
    ISqliteParameter this[string name] { get; }
    void BindAll(IntPtr statement);
}