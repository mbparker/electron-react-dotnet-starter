using LibElectronAppApi.OData.FilterExpressions;
using LibElectronAppApi.OData.FilterExpressions.Parsing;
using LibElectronAppApi.OData.Sorting;

namespace LibElectronAppApi.OData;

/// <summary>
/// Parser for OData query strings
/// </summary>
public class ODataQueryParser
{
    private const string FilterKey = "$filter";
    private const string OrderByKey = "$orderby";
    private const string TopKey = "$top";
    private const string SkipKey = "$skip";
    private const string CountKey = "$count";

    /// <summary>
    /// Parses a raw OData query string into an ODataQueryOptions object
    /// </summary>
    /// <param name="queryString">The raw query string (can include or exclude leading '?')</param>
    /// <returns>Parsed OData query options</returns>
    public static ODataQueryOptions Parse(string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return new ODataQueryOptions { RawQuery = queryString };
        }

        var options = new ODataQueryOptions { RawQuery = queryString };

        // Remove leading '?' if present
        if (queryString.StartsWith("?"))
        {
            queryString = queryString.Substring(1);
        }

        // Parse query string into key-value pairs
        var queryParams = ParseQueryString(queryString);

        // Parse each OData parameter
        foreach (var param in queryParams)
        {
            var key = param.Key.ToLowerInvariant();
            var value = param.Value;

            switch (key)
            {
                case FilterKey:
                    options.FilterRaw = value;
                    options.Filter = ParseFilter(value);
                    break;

                case OrderByKey:
                    options.OrderBy = ParseOrderBy(value);
                    break;

                case TopKey:
                    if (int.TryParse(value, out int topValue) && topValue >= 0)
                    {
                        options.Top = topValue;
                    }
                    break;

                case SkipKey:
                    if (int.TryParse(value, out int skipValue) && skipValue >= 0)
                    {
                        options.Skip = skipValue;
                    }
                    break;

                case CountKey:
                    if (bool.TryParse(value, out bool countValue))
                    {
                        options.Count = countValue;
                    }
                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// Parses the $orderby clause into a list of OrderByClause objects
    /// </summary>
    /// <param name="orderByValue">The value of the $orderby parameter</param>
    /// <returns>List of ordering clauses</returns>
    private static List<OrderByClause> ParseOrderBy(string orderByValue)
    {
        var clauses = new List<OrderByClause>();

        if (string.IsNullOrWhiteSpace(orderByValue))
        {
            return clauses;
        }

        // Split by comma to handle multiple ordering clauses
        var parts = orderByValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            var tokens = trimmedPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
            {
                continue;
            }

            var property = tokens[0];
            var direction = OrderDirection.Ascending;

            // Check if direction is specified
            if (tokens.Length > 1)
            {
                var directionStr = tokens[1].ToLowerInvariant();
                if (directionStr == "desc" || directionStr == "descending")
                {
                    direction = OrderDirection.Descending;
                }
            }

            clauses.Add(new OrderByClause(property, direction));
        }

        return clauses;
    }

    /// <summary>
    /// Parses a query string into key-value pairs
    /// </summary>
    /// <param name="queryString">The query string to parse</param>
    /// <returns>Dictionary of query parameters</returns>
    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var pairs = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split(new[] { '=' }, 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Parses an OData query string from a URI
    /// </summary>
    /// <param name="uri">The complete URI</param>
    /// <returns>Parsed OData query options</returns>
    public static ODataQueryOptions ParseFromUri(Uri uri)
    {
        if (uri == null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        return Parse(uri.Query);
    }

    /// <summary>
    /// Parses an OData filter string into a FilterExpression tree
    /// </summary>
    /// <param name="filterString">The filter string to parse</param>
    /// <returns>Parsed filter expression</returns>
    private static FilterExpression ParseFilter(string filterString)
    {
        if (string.IsNullOrWhiteSpace(filterString))
        {
            return null;
        }

        var tokenizer = new FilterTokenizer(filterString);
        var parser = new FilterExpressionParser(tokenizer);
        return parser.Parse();
    }
}