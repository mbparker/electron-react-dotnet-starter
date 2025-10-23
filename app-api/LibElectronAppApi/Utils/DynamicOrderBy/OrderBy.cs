using System.Linq.Expressions;

namespace LibElectronAppApi.Utils.DynamicOrderBy;

public class OrderBy<T, TKey> : IOrderBy
{
    private readonly Expression<Func<T, TKey>> expression;
	
    public OrderBy(Expression<Func<T, TKey>> expression)
    {
        this.expression = expression;
    }

    public dynamic Expression => expression;
}