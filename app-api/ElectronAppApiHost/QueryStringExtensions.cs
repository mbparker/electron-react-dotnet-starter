using Microsoft.AspNetCore.WebUtilities;

namespace ElectronAppApiHost;

public static class QueryStringExtensions
{
    /// <summary>
    ///  Rebuilds a query string with only the supported ODATA params.
    /// </summary>
    /// <param name="qs">The QueryString instance to extract from.</param>
    /// <returns>Url encoded string, with a leading '?'</returns>
    public static string ToODataQuery(this QueryString qs)
    {
        string[] ODataKeys = ["$filter", "$orderby", "$count", "$skip", "$top"];
        
        var dict = new Dictionary<string, string>();
        foreach (var nvp in new QueryStringEnumerable(qs.Value))
        {
            var paramName = new string(nvp.DecodeName().ToArray());
            if (ODataKeys.Contains(paramName, StringComparer.OrdinalIgnoreCase))
            {
                var paramValue = new string(nvp.DecodeValue().ToArray());
                dict.Add(paramName, paramValue);
            }
        }

        var newQs = QueryString.Create(dict);
        return newQs.ToString();
    }
}