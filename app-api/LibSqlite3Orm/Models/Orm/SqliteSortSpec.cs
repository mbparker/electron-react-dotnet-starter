using System.Data;
using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteSortSpec
{
    internal SqliteSortSpec(Expression keySelectorExpr, bool descending)
    {
        if (keySelectorExpr is LambdaExpression { Body: MemberExpression me })
            ModelMemberName = me.Member.Name;
        else
            throw new InvalidExpressionException(
                $"OrderBy, OrderByDescending, ThenBy, and ThenByDescending predicates must be of type {nameof(LambdaExpression)} with a body of {nameof(MemberExpression)}.");
        Descending = descending;
    }
        
    public string ModelMemberName { get; }
    public bool Descending { get; }
}