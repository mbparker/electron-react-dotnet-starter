namespace LibElectronAppApi.OData.FilterExpressions;

/// <summary>
/// Represents a property/field reference
/// </summary>
public class PropertyExpression : FilterExpression
{
    public string PropertyName { get; set; }

    public PropertyExpression(string propertyName)
        : base(nameof(PropertyExpression))
    {
        PropertyName = propertyName;
    }

    public override string ToString()
    {
        return PropertyName;
    }
}