using System.ComponentModel;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.ValueConverters;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataColumn : ISqliteDataColumn
{
    private readonly IntPtr statement;
    private readonly ISqliteValueConverterCache converterCache;
    private ISqliteValueConverter converterToUse;
    private object serializedValue;
    
    public SqliteDataColumn(int index, IntPtr statement, ISqliteValueConverterCache converterCache)
    {
        Index = index;
        this.statement = statement;
        this.converterCache = converterCache;
        Name = SqliteExternals.ColumnName(this.statement, Index);
        TypeAffinity = SqliteExternals.ColumnType(this.statement, Index);
        ReadSerializedValue();
    }
    
    public string Name { get; }
    public int Index { get; }
    public SqliteColType TypeAffinity { get; }
    
    public void UseConverter(ISqliteValueConverter converter)
    {
        converterToUse = converter;
    }

    public void UseConverter(Type converterType)
    {
        converterToUse = converterCache[converterType];
    }

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
        var targetType = converterToUse is not null ? converterToUse.SerializedType : type;
        var nullableType = Nullable.GetUnderlyingType(targetType);
        targetType = nullableType ?? targetType;        
        var value = DeserializeValue(targetType);
        return converterToUse?.Deserialize(value) ?? value;
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
            throw new InvalidOperationException($"Type {serializedValue.GetType()} could not be converted to {targetType}. Consider using an {nameof(ISqliteValueConverter)} implementation.");
        return result;
    }
    
    private object DeserializeInteger(Type targetType)
    {
        if (targetType == typeof(bool)) return converterCache[typeof(BooleanLong)].Deserialize(serializedValue);
        
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
        if (targetType == typeof(char)) return converterCache[typeof(CharText)].Deserialize(serializedValue);
        if (targetType == typeof(decimal)) return converterCache[typeof(DecimalText)].Deserialize(serializedValue);
        if (targetType == typeof(DateTime)) return converterCache[typeof(DateTimeText)].Deserialize(serializedValue);
        if (targetType == typeof(DateTimeOffset)) return converterCache[typeof(DateTimeOffsetText)].Deserialize(serializedValue);
        if (targetType == typeof(TimeSpan)) return converterCache[typeof(TimeSpanText)].Deserialize(serializedValue);
        if (targetType == typeof(DateOnly)) return converterCache[typeof(DateOnlyText)].Deserialize(serializedValue);
        if (targetType == typeof(TimeOnly)) return converterCache[typeof(TimeOnlyText)].Deserialize(serializedValue);
        if (targetType == typeof(Guid)) return converterCache[typeof(GuidText)].Deserialize(serializedValue);
        
        if (targetType == typeof(string)) return (string)serializedValue;
        
        return null;
    }    

    private object DeserializeBlob(Type targetType)
    {
        if (targetType == typeof(byte[])) return (byte[])serializedValue;
        
        return null;
    }
}