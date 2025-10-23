using LibElectronAppApi.OData.FilterExpressions;
using LibElectronAppApi.OData.Sorting;

namespace LibElectronAppApi.OData;

/// <summary>
/// Represents the parsed OData query options for sorting, filtering, and paging
/// </summary>
public class ODataQueryOptions
{
    /// <summary>
    /// The raw filter expression string (value of $filter parameter)
    /// </summary>
    public string FilterRaw { get; set; }

    /// <summary>
    /// Parsed filter expression tree
    /// </summary>
    public FilterExpression Filter { get; set; }

    /// <summary>
    /// List of ordering expressions parsed from $orderby
    /// </summary>
    public List<OrderByClause> OrderBy { get; set; }

    /// <summary>
    /// Maximum number of items to return (value of $top parameter)
    /// </summary>
    public int? Top { get; set; }

    /// <summary>
    /// Number of items to skip (value of $skip parameter)
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Whether to include total count (value of $count parameter)
    /// </summary>
    public bool? Count { get; set; }

    /// <summary>
    /// Raw query string that was parsed
    /// </summary>
    public string RawQuery { get; set; }

    public ODataQueryOptions()
    {
        OrderBy = new List<OrderByClause>();
    }
}