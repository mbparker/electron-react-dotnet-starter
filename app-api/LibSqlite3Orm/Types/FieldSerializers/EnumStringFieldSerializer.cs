using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Types.FieldSerializers;

public class EnumStringFieldSerializer : ISqliteEnumFieldSerializer 
{
    public Type RuntimeType => EnumType;
    public Type SerializedType => typeof(string);
    public Type EnumType { get; }

    public EnumStringFieldSerializer(Type enumType)
    {
        EnumType = enumType;
    }

    public object Serialize(object value)
    {
        return value.ToString();
    }

    public object Deserialize(object value)
    {
        return Enum.Parse(RuntimeType, (string)value, ignoreCase: true);
    }
}