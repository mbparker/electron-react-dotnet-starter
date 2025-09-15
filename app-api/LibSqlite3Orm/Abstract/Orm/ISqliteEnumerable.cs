namespace LibSqlite3Orm.Abstract.Orm;

// Don't inherit from IOrderedEnumerable<T> or IOrderedEnumerable because of naming conflicts.
public interface ISqliteEnumerable<T>
{
    IEnumerable<T> AsEnumerable();
    ISqliteEnumerable<T> Skip(int count);
    ISqliteEnumerable<T> Take(int count);
}