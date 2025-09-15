namespace LibSqlite3Orm.Abstract;

public interface ISqliteValueConverterCache
{
    ISqliteValueConverter this [Type type] { get; }
}