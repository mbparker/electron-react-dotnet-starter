namespace LibElectronAppApi.OData.FilterExpressions.Operators;

/// <summary>
/// Extension methods for operator enums
/// </summary>
public static class OperatorExtensions
{
    public static string ToODataString(this BinaryOperator op)
    {
        switch (op)
        {
            case BinaryOperator.Equal: return "eq";
            case BinaryOperator.NotEqual: return "ne";
            case BinaryOperator.GreaterThan: return "gt";
            case BinaryOperator.GreaterThanOrEqual: return "ge";
            case BinaryOperator.LessThan: return "lt";
            case BinaryOperator.LessThanOrEqual: return "le";
            case BinaryOperator.And: return "and";
            case BinaryOperator.Or: return "or";
            case BinaryOperator.Add: return "add";
            case BinaryOperator.Subtract: return "sub";
            case BinaryOperator.Multiply: return "mul";
            case BinaryOperator.Divide: return "div";
            case BinaryOperator.Modulo: return "mod";
            default: return op.ToString().ToLower();
        }
    }

    public static string ToODataString(this UnaryOperator op)
    {
        switch (op)
        {
            case UnaryOperator.Not: return "not";
            case UnaryOperator.Negate: return "-";
            default: return op.ToString().ToLower();
        }
    }
}