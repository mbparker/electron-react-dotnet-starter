using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.ValueConverters;

public class GuidText : ISqliteValueConverter
{
    public Type RuntimeType => typeof(Guid);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((Guid)value).ToString("D");
    }

    public object Deserialize(object value)
    {
        return Guid.Parse((string)value);
    }
}