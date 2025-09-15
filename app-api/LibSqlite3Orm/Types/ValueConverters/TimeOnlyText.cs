using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.ValueConverters;

public class TimeOnlyText : ISqliteValueConverter
{
    private const string Format = "HH:mm:ss.fffffff";
    
    public Type RuntimeType => typeof(TimeOnly);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((TimeOnly)value).ToString(Format);
    }

    public object Deserialize(object value)
    {
        return TimeOnly.ParseExact((string)value, Format, null);
    }
}