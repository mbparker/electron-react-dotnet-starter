namespace LibElectronAppApi.OData.FilterExpressions.Parsing;

internal enum TokenType
{
    Property,
    String,
    Number,
    Boolean,
    Null,
    Operator,
    Function,
    OpenParen,
    CloseParen,
    Comma,
    End
}