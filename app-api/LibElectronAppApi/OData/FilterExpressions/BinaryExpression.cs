using LibElectronAppApi.OData.FilterExpressions.Operators;

namespace LibElectronAppApi.OData.FilterExpressions;

/// <summary>
/// Represents a binary operation (e.g., eq, ne, gt, lt, and, or)
/// </summary>
public class BinaryExpression : FilterExpression
{
    public FilterExpression Left { get; set; }
    public BinaryOperator Operator { get; set; }
    public FilterExpression Right { get; set; }

    public BinaryExpression(FilterExpression left, BinaryOperator op, FilterExpression right)
        : base(nameof(BinaryExpression))
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override string ToString()
    {
        return $"({Left} {Operator.ToODataString()} {Right})";
    }
}