using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibElectronAppApi.OData.FilterExpressions.Operators;

/// <summary>
/// Literal value types
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LiteralType
{
    String,
    Number,
    Boolean,
    Null,
    DateTime,
    Guid
}