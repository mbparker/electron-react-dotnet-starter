using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibElectronAppApi.OData.FilterExpressions.Operators;

/// <summary>
/// Unary operators supported in OData filters
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum UnaryOperator
{
    Not,    // not
    Negate  // -
}