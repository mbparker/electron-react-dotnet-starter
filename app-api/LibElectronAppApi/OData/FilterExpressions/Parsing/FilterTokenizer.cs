using System.Text;

namespace LibElectronAppApi.OData.FilterExpressions.Parsing;

internal class FilterTokenizer
{
    private readonly string _input;
    private int _position;
    private readonly List<Token> _tokens;
    private int _currentTokenIndex;

    private static readonly HashSet<string> BinaryOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "eq", "ne", "gt", "ge", "lt", "le", "and", "or", "add", "sub", "mul", "div", "mod"
    };

    private static readonly HashSet<string> UnaryOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "not"
    };

    private static readonly HashSet<string> Functions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "contains", "startswith", "endswith", "length", "indexof", "substring",
        "tolower", "toupper", "trim", "concat", "year", "month", "day",
        "hour", "minute", "second", "date", "time", "round", "floor", "ceiling"
    };

    public FilterTokenizer(string input)
    {
        _input = input ?? string.Empty;
        _position = 0;
        _tokens = new List<Token>();
        _currentTokenIndex = 0;
        Tokenize();
    }

    private void Tokenize()
    {
        while (_position < _input.Length)
        {
            SkipWhitespace();

            if (_position >= _input.Length)
                break;

            char current = _input[_position];

            if (current == '(')
            {
                _tokens.Add(new Token(TokenType.OpenParen, "(", _position));
                _position++;
            }
            else if (current == ')')
            {
                _tokens.Add(new Token(TokenType.CloseParen, ")", _position));
                _position++;
            }
            else if (current == ',')
            {
                _tokens.Add(new Token(TokenType.Comma, ",", _position));
                _position++;
            }
            else if (current == '\'')
            {
                ReadString();
            }
            else if (char.IsDigit(current) || current == '-')
            {
                ReadNumber();
            }
            else if (char.IsLetter(current) || current == '_')
            {
                ReadIdentifier();
            }
            else
            {
                _position++; // Skip unknown character
            }
        }

        _tokens.Add(new Token(TokenType.End, string.Empty, _position));
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }
    }

    private void ReadString()
    {
        int start = _position;
        _position++; // Skip opening quote

        var sb = new StringBuilder();
        while (_position < _input.Length && _input[_position] != '\'')
        {
            if (_input[_position] == '\\' && _position + 1 < _input.Length)
            {
                _position++; // Skip escape character
            }
            sb.Append(_input[_position]);
            _position++;
        }

        if (_position < _input.Length)
        {
            _position++; // Skip closing quote
        }

        _tokens.Add(new Token(TokenType.String, sb.ToString(), start));
    }

    private void ReadNumber()
    {
        int start = _position;
        var sb = new StringBuilder();

        if (_input[_position] == '-')
        {
            sb.Append(_input[_position]);
            _position++;
        }

        while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.'))
        {
            sb.Append(_input[_position]);
            _position++;
        }

        // Is it a datetime?
        if (_position < _input.Length && "-:.TtZz".Contains(_input[_position]))
        {
            while (_position < _input.Length && (char.IsDigit(_input[_position]) || "-:.Tt".Contains(_input[_position])))
            {
                sb.Append(_input[_position]);
                _position++;
            }
            
            if (_position < _input.Length && "Zz".Contains(_input[_position]))
            {
                sb.Append(_input[_position]);
                _position++;
                _tokens.Add(new Token(TokenType.DateTime, sb.ToString(), start));
            }
            
            _tokens.Add(new Token(TokenType.String, sb.ToString(), start));
        }
        else // Check for type suffix (L, D, M, F)
        if (_position < _input.Length && "LDMFlmdf".Contains(_input[_position]))
        {
            _position++;
        }

        _tokens.Add(new Token(TokenType.Number, sb.ToString(), start));
    }

    private void ReadIdentifier()
    {
        int start = _position;
        var sb = new StringBuilder();

        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            sb.Append(_input[_position]);
            _position++;
        }

        string identifier = sb.ToString();
        TokenType type;

        if (identifier.Equals("true", StringComparison.OrdinalIgnoreCase) || 
            identifier.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            type = TokenType.Boolean;
        }
        else if (identifier.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            type = TokenType.Null;
        }
        else if (BinaryOperators.Contains(identifier) || UnaryOperators.Contains(identifier))
        {
            type = TokenType.Operator;
        }
        else if (Functions.Contains(identifier))
        {
            type = TokenType.Function;
        }
        else
        {
            type = TokenType.Property;
        }

        _tokens.Add(new Token(type, identifier, start));
    }

    public Token Current => _currentTokenIndex < _tokens.Count ? _tokens[_currentTokenIndex] : _tokens[_tokens.Count - 1];

    public Token Peek(int offset = 1)
    {
        int index = _currentTokenIndex + offset;
        return index < _tokens.Count ? _tokens[index] : _tokens[_tokens.Count - 1];
    }

    public void Advance()
    {
        if (_currentTokenIndex < _tokens.Count - 1)
        {
            _currentTokenIndex++;
        }
    }

    public bool Match(TokenType type)
    {
        return Current.Type == type;
    }

    public Token Consume(TokenType type)
    {
        if (!Match(type))
        {
            throw new InvalidOperationException($"Expected {type} but found {Current.Type} at position {Current.Position}");
        }

        var token = Current;
        Advance();
        return token;
    }
}