namespace LibElectronAppApi.OData.FilterExpressions;

/// <summary>
/// Base class for all filter expressions
/// </summary>
public abstract class FilterExpression
{
    protected FilterExpression(string typeName)
    {
        TypeName = typeName;
    }
    public string TypeName { get; set; }

    public T As<T>() where T : FilterExpression
    {
        return (T)this;
    }
}