using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibElectronAppApi.OData.Sorting;

/// <summary>
/// Order direction enumeration
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OrderDirection
{
    Ascending,
    Descending
}