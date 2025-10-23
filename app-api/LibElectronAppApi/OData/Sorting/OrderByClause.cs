namespace LibElectronAppApi.OData.Sorting;

/// <summary>
/// Represents a single ordering clause from $orderby
/// </summary>
public class OrderByClause
{
    /// <summary>
    /// The property/field name to order by
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// The direction of ordering (asc or desc)
    /// </summary>
    public OrderDirection Direction { get; set; }

    public OrderByClause(string property, OrderDirection direction = OrderDirection.Ascending)
    {
        Property = property;
        Direction = direction;
    }

    public override string ToString()
    {
        return $"{Property} {Direction.ToString().ToLower()}";
    }
}