using System.ComponentModel;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataColumn : ISqliteDataColumn
{
    private readonly IntPtr statement;
    private readonly ISqliteFieldValueSerialization serialization;
    private object serializedValue;
    
    public SqliteDataColumn(int index, IntPtr statement, ISqliteFieldValueSerialization serialization)
    {
        Index = index;
        this.statement = statement;
        this.serialization = serialization;
        Name = SqliteExternals.ColumnName(this.statement, Index);
        TypeAffinity = SqliteExternals.ColumnType(this.statement, Index);
        ReadSerializedValue();
    }
    
    public string Name { get; }
    public int Index { get; }
    public SqliteColType TypeAffinity { get; }

    public object Value()
    {
        return serializedValue;
    }

    public T ValueAs<T>()
    {
        return (T)ValueAs(typeof(T));
    }
    
    public object ValueAs(Type type)
    {
        if (serializedValue is null) return null;
        var nullableType = Nullable.GetUnderlyingType(type);
        type = nullableType ?? type;        
        var value = DeserializeValue(type);
        return value;
    }    

    private void ReadSerializedValue()
    {
        switch (TypeAffinity)
        {
            case SqliteColType.Integer:
                serializedValue = SqliteExternals.ColumnInt64(statement, Index);
                break;
            case SqliteColType.Float:
                serializedValue = SqliteExternals.ColumnDouble(statement, Index);
                break;
            case SqliteColType.Text:
                serializedValue = SqliteExternals.ColumnText(statement, Index);
                if (serializedValue is not null)
                    serializedValue = ((string)serializedValue).Utf8ToUnicode();
                break;
            case SqliteColType.Blob:
                serializedValue = SqliteExternals.ColumnBlob(statement, Index);
                break;
            case SqliteColType.Null:
                serializedValue = null;
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(TypeAffinity), (int)TypeAffinity,
                    typeof(SqliteColType));
        }
    }

    private object DeserializeValue(Type targetType)
    {
        object result = null;
        switch (TypeAffinity)
        {
            case SqliteColType.Integer:
                result = DeserializeInteger(targetType);
                break;
            case SqliteColType.Float:
                result = DeserializeDouble(targetType);
                break;
            case SqliteColType.Text:
                result = DeserializeText(targetType);
                break;
            case SqliteColType.Blob:
                result = DeserializeBlob(targetType);
                break;
        }
        
        if (result is null)
            throw new InvalidOperationException($"Type {serializedValue.GetType()} could not be converted to {targetType}. Consider using an {nameof(ISqliteFieldSerializer)} implementation.");
        return result;
    }
    
    private object DeserializeInteger(Type targetType)
    {
        if (targetType == typeof(bool)) return serialization[typeof(bool)].Deserialize(serializedValue);
        
        if (targetType == typeof(sbyte)) return Convert.ToSByte(serializedValue);
        if (targetType == typeof(short)) return Convert.ToInt16(serializedValue);
        if (targetType == typeof(int)) return Convert.ToInt32(serializedValue);
        if (targetType == typeof(long)) return (long)serializedValue;
        if (targetType == typeof(byte)) return Convert.ToByte(serializedValue);
        if (targetType == typeof(ushort)) return Convert.ToUInt16(serializedValue);
        if (targetType == typeof(uint)) return Convert.ToUInt32(serializedValue);
        if (targetType == typeof(ulong)) return BitConverter.ToUInt64(BitConverter.GetBytes((long)serializedValue));

        return null;
    }
    
    private object DeserializeDouble(Type targetType)
    {
        if (targetType == typeof(float)) return Convert.ToDouble(serializedValue);
        if (targetType == typeof(double)) return (double)serializedValue;
        
        return null;
    }

    private object DeserializeText(Type targetType)
    {
        if (targetType.IsEnum) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(char)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(decimal)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(DateTime)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(DateTimeOffset)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(TimeSpan)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(DateOnly)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(TimeOnly)) return serialization[targetType].Deserialize(serializedValue);
        if (targetType == typeof(Guid)) return serialization[targetType].Deserialize(serializedValue);
        
        if (targetType == typeof(string)) return (string)serializedValue;
        
        return null;
    }    

    private object DeserializeBlob(Type targetType)
    {
        if (targetType == typeof(byte[])) return (byte[])serializedValue;
        
        return null;
    }
}