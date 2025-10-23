namespace LibElectronAppApi.OData.FilterExpressions.Operators;

/// <summary>
/// Binary operators supported in OData filters
/// </summary>
public enum BinaryOperator
{
    Equal,              // eq
    NotEqual,           // ne
    GreaterThan,        // gt
    GreaterThanOrEqual, // ge
    LessThan,           // lt
    LessThanOrEqual,    // le
    And,                // and
    Or,                 // or
    Add,                // add
    Subtract,           // sub
    Multiply,           // mul
    Divide,             // div
    Modulo              // mod
}