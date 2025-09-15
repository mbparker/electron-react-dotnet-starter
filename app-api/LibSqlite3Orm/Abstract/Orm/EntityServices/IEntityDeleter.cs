using System.Linq.Expressions;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDeleter
{
    int Delete<T>(Expression<Func<T, bool>> predicate);
    int DeleteAll<T>();
}