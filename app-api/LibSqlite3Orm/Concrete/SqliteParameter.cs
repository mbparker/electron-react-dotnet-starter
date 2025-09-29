using System.ComponentModel;
using System.Text;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete;

public class SqliteParameter : ISqliteParameter, ISqliteParameterDebug
{
    private static readonly IntPtr NoDeallocator = new(-1);
    private readonly ISqliteFieldValueSerialization serialization;
    
    public SqliteParameter(string name, int index, ISqliteFieldValueSerialization serialization)
    {
        Name = name;
        Index = index;
        this.serialization = serialization;
    }
    
    public string Name { get; }
    public int Index { get; }
    
    public object DeserializedValue { get; private set; }
    public object SerialzedValue { get; private set; }
    public SqliteColType SerializedTypeAffinity { get; private set; }

    public void Set(object value)
    {
        DeserializedValue = value;
        SerializeValue();
    }

    string ISqliteParameterDebug.GetDebugValue()
    {
        if (SerialzedValue is null) return "NULL";
        if (SerialzedValue.GetType() == typeof(byte[]))
            return Convert.ToHexString((byte[])SerialzedValue);
        return SerialzedValue.ToString();
    }

    public void Bind(IntPtr statement)
    {
        switch (SerializedTypeAffinity)
        {
            case SqliteColType.Integer:
                var intType = SerialzedValue.GetType();
                if (intType == typeof(long))
                    SqliteExternals.BindInt64(statement, Index, (long)SerialzedValue);
                else
                    SqliteExternals.BindInt64(statement, Index, Convert.ToInt64(SerialzedValue));
                break;  
            case SqliteColType.Float:
                var realType = SerialzedValue.GetType();
                if (realType == typeof(double))
                    SqliteExternals.BindDouble(statement, Index, (double)SerialzedValue);
                else
                    SqliteExternals.BindDouble(statement, Index, Convert.ToDouble(SerialzedValue));
                break;
            case SqliteColType.Text:
                var s = ((string)SerialzedValue).UnicodeToUtf8();
                var n = Encoding.UTF8.GetByteCount(s);
                SqliteExternals.BindText(statement, Index, s, n, NoDeallocator);
                break;
            case SqliteColType.Blob:
                var blob = (byte[])SerialzedValue;
                SqliteExternals.BindBlob(statement, Index, blob, blob.Length, NoDeallocator);
                break;
            case SqliteColType.Null:
                SqliteExternals.BindNull(statement, Index);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(SerializedTypeAffinity), (int)SerializedTypeAffinity,
                    typeof(SqliteColType));
        }
    }

    private void SerializeValue()
    {
        if (DeserializedValue is null)
        {
            SerializedTypeAffinity = SqliteColType.Null;
            SerialzedValue = null;
            return;
        }

        var serializer = serialization[DeserializedValue.GetType()];
        if (serializer is not null)
            SerialzedValue = serializer.Serialize(DeserializedValue);
        else
            SerialzedValue = DeserializedValue; // No serializer - might be storable as is - check below
        
        var type = SerialzedValue.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;

        var affinity = GetSerializedTypeAffinity(type);
        if (affinity.HasValue)
        {
            SerializedTypeAffinity = affinity.Value;
            return;
        }

        throw new InvalidOperationException(
            $"Type {type} is not supported. Consider registering an {nameof(ISqliteFieldSerializer)} implementation " +
            "to serialize that type to a type that can be stored.");
    }

    // These are the only fundamental types that can be stored in SQLite. Anything else must be serialized to one of these.
    private SqliteColType? GetSerializedTypeAffinity(Type type)
    {
        if (type == typeof(string))
        {
            return SqliteColType.Text;
        }

        if (type == typeof(byte))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(ushort))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(uint))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(sbyte))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(short))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(int))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(long))
        {
            return SqliteColType.Integer;
        }

        if (type == typeof(double))
        {
            return SqliteColType.Float;
        }

        if (type == typeof(byte[]))
        {
            return SqliteColType.Blob;
        }

        return null;
    }
}