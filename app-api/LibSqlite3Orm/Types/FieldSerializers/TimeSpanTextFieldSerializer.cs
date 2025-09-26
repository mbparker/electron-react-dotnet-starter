using System.Globalization;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class TimeSpanTextFieldSerializer : ISqliteFieldSerializer
{
    private const string Format = "d.hh:mm:ss.fffffff";
    
    public Type RuntimeType => typeof(TimeSpan);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((TimeSpan)value).ToString(Format);
    }

    public object Deserialize(object value)
    {
        return TimeSpan.ParseExact((string)value, Format, null, TimeSpanStyles.None);
    }
}