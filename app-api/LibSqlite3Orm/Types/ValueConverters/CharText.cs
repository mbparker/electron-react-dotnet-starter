using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.ValueConverters;

public class CharText : ISqliteValueConverter
{
    public Type RuntimeType => typeof(char);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return ((char)value).ToString();
    }

    public object Deserialize(object value)
    {
        return ((string)value).ToCharArray().First();
    }
}