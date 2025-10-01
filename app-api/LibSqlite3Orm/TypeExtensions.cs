using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm;

public static class TypeExtensions
{
    // These are the only fundamental types that can be stored in SQLite. Anything else must be serialized to one of these.
    public static SqliteDataType? GetSqliteDataType(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            //case TypeCode.UInt64: - Values can overflow. Must be converted to string to be useful in a stored state.
                return SqliteDataType.Integer;
            case TypeCode.Double:
            case TypeCode.Single:
                return SqliteDataType.Float;
            case TypeCode.String:
            case TypeCode.Char:
                return SqliteDataType.Text;
            default:
                if (type.IsArray && Type.GetTypeCode(type.GetElementType()) == TypeCode.Byte)
                    return SqliteDataType.Blob;
                return null;
        }
    }
}