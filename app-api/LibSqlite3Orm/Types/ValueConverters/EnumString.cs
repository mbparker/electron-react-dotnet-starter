using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.ValueConverters;

public class EnumString<TEnum> : ISqliteValueConverter where TEnum : Enum 
{
    public Type RuntimeType => typeof(TEnum);
    public Type SerializedType => typeof(string);
    
    public object Serialize(object value)
    {
        return value.ToString();
    }

    public object Deserialize(object value)
    {
        return Enum.Parse(RuntimeType, (string)value, ignoreCase: true);
    }
}