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
    private object serialzedValue;
    private SqliteColType serializedTypeAffinity = SqliteColType.Null;
    private ISqliteFieldSerializer serializerToUse;
    private ISqliteFieldSerializer fallbackSerializer;
    
    public SqliteParameter(string name, int index, ISqliteFieldValueSerialization serialization)
    {
        Name = name;
        Index = index;
        this.serialization = serialization;
    }
    
    public string Name { get; }
    public int Index { get; }

    public void UseSerializer(ISqliteFieldSerializer serializer)
    {
        serializerToUse = serializer;
    }
    
    public void UseSerializer(Type modelType)
    {
        serializerToUse = serialization[modelType];
    }

    public void Set(object value)
    {
        SerializeValue(value);
    }

    string ISqliteParameterDebug.GetDebugValue()
    {
        if (serialzedValue is null) return "NULL";
        if (serialzedValue.GetType() == typeof(byte[]))
            return Convert.ToHexString((byte[])serialzedValue);
        return serialzedValue.ToString();
    }

    public void Bind(IntPtr statement)
    {
        switch (serializedTypeAffinity)
        {
            case SqliteColType.Integer:
                var intType = serialzedValue.GetType();
                if (intType == typeof(long))
                    SqliteExternals.BindInt64(statement, Index, (long)serialzedValue);
                else if (intType == typeof(ulong))
                    SqliteExternals.BindInt64(statement, Index, BitConverter.ToInt64(BitConverter.GetBytes((ulong)serialzedValue)));
                else
                    SqliteExternals.BindInt64(statement, Index, Convert.ToInt64(serialzedValue));
                break;  
            case SqliteColType.Float:
                var realType = serialzedValue.GetType();
                if (realType == typeof(double))
                    SqliteExternals.BindDouble(statement, Index, (double)serialzedValue);
                else
                    SqliteExternals.BindDouble(statement, Index, Convert.ToDouble(serialzedValue));
                break;
            case SqliteColType.Text:
                var s = ((string)serialzedValue).UnicodeToUtf8();
                var n = Encoding.UTF8.GetByteCount(s);
                SqliteExternals.BindText(statement, Index, s, n, NoDeallocator);
                break;
            case SqliteColType.Blob:
                var blob = (byte[])serialzedValue;
                SqliteExternals.BindBlob(statement, Index, blob, blob.Length, NoDeallocator);
                break;
            case SqliteColType.Null:
                SqliteExternals.BindNull(statement, Index);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(serializedTypeAffinity), (int)serializedTypeAffinity,
                    typeof(SqliteColType));
        }
    }

    private void SerializeValue(object value)
    {
        fallbackSerializer = null;
        
        if (value is null)
        {
            serializedTypeAffinity = SqliteColType.Null;
            serialzedValue = null;
            return;
        }

        if (serializerToUse is not null)
            serialzedValue = serializerToUse.Serialize(value);
        else
            serialzedValue = value;
        
        var type = serialzedValue.GetType();
        var nullableType = Nullable.GetUnderlyingType(type);
        type = nullableType ?? type;

        if (type == typeof(string))
        {
            serializedTypeAffinity = SqliteColType.Text;
            return;
        }

        if (type == typeof(byte))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(ushort))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(uint))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(ulong))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(sbyte))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(short))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(int))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(long))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            return;
        }

        if (type == typeof(float))
        {
            serializedTypeAffinity = SqliteColType.Float;
            return;
        }

        if (type == typeof(double))
        {
            serializedTypeAffinity = SqliteColType.Float;
            return;
        }

        if (type == typeof(byte[]))
        {
            serializedTypeAffinity = SqliteColType.Blob;
            return;
        }
        
        // These cannot be stored as-is. The caller neglected to specify a serializer, or chose an invalid serializer.

        if (type == typeof(bool))
        {
            serializedTypeAffinity = SqliteColType.Integer;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }
        
        if (type == typeof(char))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }        
        
        if (type == typeof(decimal))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(DateTime))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(DateTimeOffset))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(TimeSpan))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(DateOnly))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(TimeOnly))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type == typeof(Guid))
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }

        if (type.IsEnum)
        {
            serializedTypeAffinity = SqliteColType.Text;
            fallbackSerializer = serialization[type];
            serialzedValue = fallbackSerializer.Serialize(serialzedValue);
            return;
        }
        
        throw new InvalidOperationException($"Type {type} is not supported. Consider using an {nameof(ISqliteFieldSerializer)} implementation.");
    }
}