using System.Globalization;
using LibElectronAppApi.OData.FilterExpressions.Operators;

namespace LibElectronAppApi.OData.FilterExpressions.Parsing;

internal class FilterExpressionParser
{
    private readonly FilterTokenizer _tokenizer;

    public FilterExpressionParser(FilterTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public FilterExpression Parse()
    {
        return ParseOrExpression();
    }

    private FilterExpression ParseOrExpression()
    {
        var left = ParseAndExpression();

        while (_tokenizer.Match(TokenType.Operator) && 
               _tokenizer.Current.Value.Equals("or", StringComparison.OrdinalIgnoreCase))
        {
            _tokenizer.Advance();
            var right = ParseAndExpression();
            left = new BinaryExpression(left, BinaryOperator.Or, right);
        }

        return left;
    }

    private FilterExpression ParseAndExpression()
    {
        var left = ParseComparisonExpression();

        while (_tokenizer.Match(TokenType.Operator) && 
               _tokenizer.Current.Value.Equals("and", StringComparison.OrdinalIgnoreCase))
        {
            _tokenizer.Advance();
            var right = ParseComparisonExpression();
            left = new BinaryExpression(left, BinaryOperator.And, right);
        }

        return left;
    }

    private FilterExpression ParseComparisonExpression()
    {
        var left = ParseAdditiveExpression();

        if (_tokenizer.Match(TokenType.Operator))
        {
            var opToken = _tokenizer.Current.Value.ToLowerInvariant();
            BinaryOperator? op = null;

            switch (opToken)
            {
                case "eq": op = BinaryOperator.Equal; break;
                case "ne": op = BinaryOperator.NotEqual; break;
                case "gt": op = BinaryOperator.GreaterThan; break;
                case "ge": op = BinaryOperator.GreaterThanOrEqual; break;
                case "lt": op = BinaryOperator.LessThan; break;
                case "le": op = BinaryOperator.LessThanOrEqual; break;
            }

            if (op.HasValue)
            {
                _tokenizer.Advance();
                var right = ParseAdditiveExpression();
                return new BinaryExpression(left, op.Value, right);
            }
        }

        return left;
    }

    private FilterExpression ParseAdditiveExpression()
    {
        var left = ParseMultiplicativeExpression();

        while (_tokenizer.Match(TokenType.Operator))
        {
            var opToken = _tokenizer.Current.Value.ToLowerInvariant();
            BinaryOperator? op = null;

            if (opToken == "add") op = BinaryOperator.Add;
            else if (opToken == "sub") op = BinaryOperator.Subtract;

            if (op.HasValue)
            {
                _tokenizer.Advance();
                var right = ParseMultiplicativeExpression();
                left = new BinaryExpression(left, op.Value, right);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private FilterExpression ParseMultiplicativeExpression()
    {
        var left = ParseUnaryExpression();

        while (_tokenizer.Match(TokenType.Operator))
        {
            var opToken = _tokenizer.Current.Value.ToLowerInvariant();
            BinaryOperator? op = null;

            if (opToken == "mul") op = BinaryOperator.Multiply;
            else if (opToken == "div") op = BinaryOperator.Divide;
            else if (opToken == "mod") op = BinaryOperator.Modulo;

            if (op.HasValue)
            {
                _tokenizer.Advance();
                var right = ParseUnaryExpression();
                left = new BinaryExpression(left, op.Value, right);
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private FilterExpression ParseUnaryExpression()
    {
        if (_tokenizer.Match(TokenType.Operator) && 
            _tokenizer.Current.Value.Equals("not", StringComparison.OrdinalIgnoreCase))
        {
            _tokenizer.Advance();
            var operand = ParseUnaryExpression();
            return new UnaryExpression(UnaryOperator.Not, operand);
        }

        return ParsePrimaryExpression();
    }

    private FilterExpression ParsePrimaryExpression()
    {
        // Function call
        if (_tokenizer.Match(TokenType.Function))
        {
            return ParseFunctionCall();
        }

        // Parenthesized expression
        if (_tokenizer.Match(TokenType.OpenParen))
        {
            _tokenizer.Advance();
            var expr = ParseOrExpression();
            _tokenizer.Consume(TokenType.CloseParen);
            return expr;
        }

        // Literal values
        if (_tokenizer.Match(TokenType.String))
        {
            var value = _tokenizer.Current.Value;
            _tokenizer.Advance();
            return new LiteralExpression(value, LiteralType.String);
        }

        if (_tokenizer.Match(TokenType.DateTime))
        {
            var value = _tokenizer.Current.Value;
            _tokenizer.Advance();
            if (DateTime.TryParse(value, null, DateTimeStyles.AdjustToUniversal, out var dateTimeValue))
                return new LiteralExpression(dateTimeValue, LiteralType.DateTime);
        }

        if (_tokenizer.Match(TokenType.Number))
        {
            var value = _tokenizer.Current.Value;
            _tokenizer.Advance();

            // Handle hex number
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                var hexString = value.Remove(0, 2);
                
                if ("ULul".Contains(hexString[^1]))
                {
                    if (hexString.EndsWith("ul", StringComparison.OrdinalIgnoreCase) ||
                        hexString.EndsWith("lu", StringComparison.OrdinalIgnoreCase))
                        return new LiteralExpression(ulong.Parse(hexString.Remove(hexString.Length - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture), LiteralType.Number);
                    var valueOnly = hexString.Remove(hexString.Length - 1);
                    switch (char.ToLower(hexString[^1]))
                    {
                        case 'l':
                            return new LiteralExpression(long.Parse(valueOnly, NumberStyles.HexNumber, CultureInfo.InvariantCulture), LiteralType.Number);
                        case 'u':
                            return new LiteralExpression(ulong.Parse(valueOnly, NumberStyles.HexNumber, CultureInfo.InvariantCulture), LiteralType.Number);

                    }
                }
                
                if (int.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var intValue))
                    return new LiteralExpression(intValue, LiteralType.Number);
                if (long.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var longValue))
                    return new LiteralExpression(longValue, LiteralType.Number);
            }

            // Handle 2 char suffix - unsigned long
            if (value.EndsWith("ul", StringComparison.OrdinalIgnoreCase) || value.EndsWith("lu", StringComparison.OrdinalIgnoreCase))
                return new LiteralExpression(ulong.Parse(value.Remove(value.Length - 2)), LiteralType.Number);
            
            // Handle supported numeric suffixes
            if ("ULDMFulmdf".Contains(value[^1]))
            {
                var valueOnly = value.Remove(value.Length - 1);
                switch (char.ToLower(value[^1]))
                {
                    case 'f':
                        return new LiteralExpression(float.Parse(valueOnly), LiteralType.Number);
                    case 'd':
                        return new LiteralExpression(double.Parse(valueOnly), LiteralType.Number);
                    case 'm':
                        return new LiteralExpression(decimal.Parse(valueOnly), LiteralType.Number);
                    case 'l':
                        return new LiteralExpression(long.Parse(valueOnly), LiteralType.Number);
                    case 'u':
                        return new LiteralExpression(ulong.Parse(valueOnly), LiteralType.Number);

                }
            }
                
            // Yay! Now we get to guess!
            if (value.Contains("."))
            {
                // We know it's a real number
                if (float.TryParse(value, out var floatValue))
                    return new LiteralExpression(floatValue, LiteralType.Number);
                if (double.TryParse(value, out var doubleValue))
                    return new LiteralExpression(doubleValue, LiteralType.Number);
                if (decimal.TryParse(value, out var decimalValue))
                    return new LiteralExpression(decimalValue, LiteralType.Number);
            }
            else
            {
                // It's an integer
                if (int.TryParse(value, out var intValue))
                    return new LiteralExpression(intValue, LiteralType.Number);
                if (long.TryParse(value, out var longValue))
                    return new LiteralExpression(longValue, LiteralType.Number);
            }

            // Wtf did you pass me??
            throw new ApplicationException($"Number value: {value} cannot be parsed by any supported numeric types.");
        }

        if (_tokenizer.Match(TokenType.Boolean))
        {
            var value = bool.Parse(_tokenizer.Current.Value);
            _tokenizer.Advance();
            return new LiteralExpression(value, LiteralType.Boolean);
        }

        if (_tokenizer.Match(TokenType.Null))
        {
            _tokenizer.Advance();
            return new LiteralExpression(null, LiteralType.Null);
        }

        // Property reference
        if (_tokenizer.Match(TokenType.Property))
        {
            var propertyName = _tokenizer.Current.Value;
            _tokenizer.Advance();
            return new PropertyExpression(propertyName);
        }

        throw new InvalidOperationException($"Unexpected token: {_tokenizer.Current}");
    }

    private FilterExpression ParseFunctionCall()
    {
        var functionName = _tokenizer.Current.Value;
        _tokenizer.Advance();

        _tokenizer.Consume(TokenType.OpenParen);

        var arguments = new List<FilterExpression>();

        if (!_tokenizer.Match(TokenType.CloseParen))
        {
            arguments.Add(ParseOrExpression());

            while (_tokenizer.Match(TokenType.Comma))
            {
                _tokenizer.Advance();
                arguments.Add(ParseOrExpression());
            }
        }

        _tokenizer.Consume(TokenType.CloseParen);

        return new FunctionExpression(functionName, arguments);
    }
}