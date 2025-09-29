using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Abstract;

public interface ISqliteDataColumn
{
    string Name { get; }
    int Index { get; }
    SqliteColType TypeAffinity { get; }
    object Value();
    T ValueAs<T>();
    object ValueAs(Type type);
}