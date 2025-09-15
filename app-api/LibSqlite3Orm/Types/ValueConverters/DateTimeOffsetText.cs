using System.Globalization;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.ValueConverters;

public class DateTimeOffsetText : ISqliteValueConverter
{
    private const string Format = "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz";
    
    public Type RuntimeType => typeof(DateTimeOffset);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((DateTimeOffset)value).ToString(Format);
    }

    public object Deserialize(object value)
    {
        return DateTimeOffset.ParseExact((string)value, Format, null, DateTimeStyles.None);
    }
}