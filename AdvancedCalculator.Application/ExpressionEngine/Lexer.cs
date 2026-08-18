using System.Globalization;
using System.Text;
using AdvancedCalculator.Core.Exceptions;

namespace AdvancedCalculator.Application.ExpressionEngine;

public class Lexer
{
    private static readonly HashSet<char> OperatorChars = new() { '+', '-', '*', '/', '×', '÷', '^', '%', '!' };

    public static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        if (string.IsNullOrWhiteSpace(expression))
            return tokens;

        int i = 0;
        int length = expression.Length;

        while (i < length)
        {
            char c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                var sb = new StringBuilder();
                bool hasDecimal = false;

                while (i < length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    if (expression[i] == '.')
                    {
                        if (hasDecimal)
                            throw new InvalidExpressionCustomException("Multiple decimal points in number");
                        hasDecimal = true;
                    }
                    sb.Append(expression[i]);
                    i++;
                }

                // Check for scientific notation e.g. 1.5e-3 or 2E+5
                if (i < length && (expression[i] == 'e' || expression[i] == 'E'))
                {
                    int nextIdx = i + 1;
                    if (nextIdx < length && (expression[nextIdx] == '+' || expression[nextIdx] == '-' || char.IsDigit(expression[nextIdx])))
                    {
                        sb.Append(expression[i]); // 'e'
                        i++;
                        if (expression[i] == '+' || expression[i] == '-')
                        {
                            sb.Append(expression[i]);
                            i++;
                        }
                        while (i < length && char.IsDigit(expression[i]))
                        {
                            sb.Append(expression[i]);
                            i++;
                        }
                    }
                }

                tokens.Add(new Token(TokenType.Number, sb.ToString()));
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                i++;
                continue;
            }

            if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ","));
                i++;
                continue;
            }

            if (OperatorChars.Contains(c))
            {
                string op = c switch
                {
                    '×' => "*",
                    '÷' => "/",
                    _ => c.ToString()
                };

                // Check for unary minus / plus
                bool isUnary = false;
                if (op == "-" || op == "+")
                {
                    if (tokens.Count == 0 ||
                        tokens[^1].Type == TokenType.Operator ||
                        tokens[^1].Type == TokenType.LeftParen ||
                        tokens[^1].Type == TokenType.Comma)
                    {
                        isUnary = true;
                    }
                }

                if (isUnary && op == "-")
                {
                    tokens.Add(new Token(TokenType.Operator, "u-"));
                }
                else if (isUnary && op == "+")
                {
                    // Unary plus can simply be ignored
                }
                else
                {
                    tokens.Add(new Token(TokenType.Operator, op));
                }

                i++;
                continue;
            }

            if (char.IsLetter(c) || c == 'π' || c == '√')
            {
                if (c == 'π')
                {
                    tokens.Add(new Token(TokenType.Number, Math.PI.ToString(CultureInfo.InvariantCulture)));
                    i++;
                    continue;
                }
                if (c == '√')
                {
                    tokens.Add(new Token(TokenType.Function, "sqrt"));
                    i++;
                    continue;
                }

                var sb = new StringBuilder();
                while (i < length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                {
                    sb.Append(expression[i]);
                    i++;
                }

                string name = sb.ToString().ToLowerInvariant();

                // Constants
                if (name == "pi")
                {
                    tokens.Add(new Token(TokenType.Number, Math.PI.ToString(CultureInfo.InvariantCulture)));
                }
                else if (name == "e")
                {
                    tokens.Add(new Token(TokenType.Number, Math.E.ToString(CultureInfo.InvariantCulture)));
                }
                else if (name == "phi")
                {
                    tokens.Add(new Token(TokenType.Number, "1.618033988749895"));
                }
                else
                {
                    // Function or named operator
                    tokens.Add(new Token(TokenType.Function, name));
                }
                continue;
            }

            throw new InvalidExpressionCustomException($"Unexpected character: '{c}'");
        }

        // Implicit multiplication insertion: e.g. 2(3) -> 2*(3), 2sin(30) -> 2*sin(30), (2)(3) -> (2)*(3)
        var result = new List<Token>();
        for (int j = 0; j < tokens.Count; j++)
        {
            var curr = tokens[j];
            result.Add(curr);

            if (j < tokens.Count - 1)
            {
                var next = tokens[j + 1];

                bool needsMultiply =
                    (curr.Type == TokenType.Number && (next.Type == TokenType.LeftParen || next.Type == TokenType.Function)) ||
                    (curr.Type == TokenType.RightParen && (next.Type == TokenType.Number || next.Type == TokenType.LeftParen || next.Type == TokenType.Function)) ||
                    (curr.Type == TokenType.Operator && curr.Value == "!" && (next.Type == TokenType.Number || next.Type == TokenType.LeftParen || next.Type == TokenType.Function));

                if (needsMultiply)
                {
                    result.Add(new Token(TokenType.Operator, "*"));
                }
            }
        }

        return result;
    }
}
