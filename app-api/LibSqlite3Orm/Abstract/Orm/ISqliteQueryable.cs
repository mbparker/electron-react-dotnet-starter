using System.Linq.Expressions;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteQueryable<T> : ISqliteEnumerable<T>
{
    ISqliteQueryable<T> Where(Expression<Func<T, bool>> predicate);
    ISqliteOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
    ISqliteOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
}