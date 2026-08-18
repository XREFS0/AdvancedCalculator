using System.Globalization;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Exceptions;
using AdvancedCalculator.Core.Interfaces;

namespace AdvancedCalculator.Application.ExpressionEngine;

public class ExpressionEngine : IExpressionEngine
{
    private static readonly Dictionary<string, (int Precedence, bool RightAssociative)> Operators = new()
    {
        { "u-", (5, true) },   // Unary minus
        { "!",  (5, false) },  // Factorial postfix
        { "^",  (4, true) },   // Power
        { "*",  (3, false) },  // Multiply
        { "/",  (3, false) },  // Divide
        { "%",  (3, false) },  // Modulus
        { "+",  (2, false) },  // Add
        { "-",  (2, false) }   // Subtract
    };

    private static readonly HashSet<string> Functions = new()
    {
        "sin", "cos", "tan",
        "asin", "acos", "atan",
        "sinh", "cosh", "tanh",
        "asinh", "acosh", "atanh",
        "log", "log10", "ln", "exp",
        "sqrt", "cbrt", "abs", "floor", "ceil", "round",
        "deg", "rad", "grad",
        "fact"
    };

    public double Evaluate(string expression, AngleMode angleMode = AngleMode.Degrees)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return 0;

        var tokens = Lexer.Tokenize(expression);
        var rpn = ConvertToRpn(tokens);
        return EvaluateRpn(rpn, angleMode);
    }

    public bool TryEvaluate(string expression, out double result, out string errorMessage, AngleMode angleMode = AngleMode.Degrees)
    {
        result = 0;
        errorMessage = string.Empty;
        try
        {
            result = Evaluate(expression, angleMode);
            if (double.IsNaN(result))
            {
                errorMessage = "Invalid calculation result (NaN)";
                return false;
            }
            if (double.IsInfinity(result))
            {
                errorMessage = "Cannot divide by zero or result is infinite";
                return false;
            }
            return true;
        }
        catch (CalculationException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
            return false;
        }
    }

    private static List<Token> ConvertToRpn(List<Token> tokens)
    {
        var output = new List<Token>();
        var opStack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    output.Add(token);
                    break;

                case TokenType.Function:
                    opStack.Push(token);
                    break;

                case TokenType.Comma:
                    while (opStack.Count > 0 && opStack.Peek().Type != TokenType.LeftParen)
                    {
                        output.Add(opStack.Pop());
                    }
                    if (opStack.Count == 0 || opStack.Peek().Type != TokenType.LeftParen)
                        throw new InvalidExpressionCustomException("Misplaced comma or mismatched parentheses");
                    break;

                case TokenType.Operator:
                    if (!Operators.TryGetValue(token.Value, out var currOpInfo))
                        throw new InvalidExpressionCustomException($"Unknown operator: '{token.Value}'");

                    while (opStack.Count > 0 && opStack.Peek().Type == TokenType.Operator)
                    {
                        var topOp = opStack.Peek().Value;
                        if (Operators.TryGetValue(topOp, out var topOpInfo))
                        {
                            if ((!currOpInfo.RightAssociative && currOpInfo.Precedence <= topOpInfo.Precedence) ||
                                (currOpInfo.RightAssociative && currOpInfo.Precedence < topOpInfo.Precedence))
                            {
                                output.Add(opStack.Pop());
                                continue;
                            }
                        }
                        break;
                    }
                    opStack.Push(token);
                    break;

                case TokenType.LeftParen:
                    opStack.Push(token);
                    break;

                case TokenType.RightParen:
                    bool matched = false;
                    while (opStack.Count > 0)
                    {
                        if (opStack.Peek().Type == TokenType.LeftParen)
                        {
                            opStack.Pop(); // Discard '('
                            matched = true;
                            break;
                        }
                        output.Add(opStack.Pop());
                    }
                    if (!matched)
                        throw new InvalidExpressionCustomException("Mismatched parentheses");

                    if (opStack.Count > 0 && opStack.Peek().Type == TokenType.Function)
                    {
                        output.Add(opStack.Pop());
                    }
                    break;
            }
        }

        while (opStack.Count > 0)
        {
            var top = opStack.Pop();
            if (top.Type == TokenType.LeftParen || top.Type == TokenType.RightParen)
                throw new InvalidExpressionCustomException("Mismatched parentheses in expression");
            output.Add(top);
        }

        return output;
    }

    private static double EvaluateRpn(List<Token> rpn, AngleMode angleMode)
    {
        var valStack = new Stack<double>();

        foreach (var token in rpn)
        {
            if (token.Type == TokenType.Number)
            {
                if (!double.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                    throw new InvalidExpressionCustomException($"Invalid number literal: '{token.Value}'");
                valStack.Push(val);
            }
            else if (token.Type == TokenType.Operator)
            {
                if (token.Value == "u-")
                {
                    if (valStack.Count < 1) throw new InvalidExpressionCustomException("Missing operand for negation");
                    valStack.Push(-valStack.Pop());
                }
                else if (token.Value == "!")
                {
                    if (valStack.Count < 1) throw new InvalidExpressionCustomException("Missing operand for factorial");
                    valStack.Push(Factorial(valStack.Pop()));
                }
                else
                {
                    if (valStack.Count < 2) throw new InvalidExpressionCustomException($"Missing operand for '{token.Value}'");
                    double b = valStack.Pop();
                    double a = valStack.Pop();

                    double res = token.Value switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => b == 0 ? throw new DivisionByZeroCustomException() : a / b,
                        "%" => b == 0 ? throw new DivisionByZeroCustomException() : a % b,
                        "^" => Math.Pow(a, b),
                        _ => throw new InvalidExpressionCustomException($"Unsupported operator: '{token.Value}'")
                    };
                    valStack.Push(res);
                }
            }
            else if (token.Type == TokenType.Function)
            {
                if (valStack.Count < 1) throw new InvalidExpressionCustomException($"Missing argument for function '{token.Value}'");
                double arg = valStack.Pop();

                double res = EvaluateFunction(token.Value, arg, angleMode);
                valStack.Push(res);
            }
        }

        if (valStack.Count != 1)
            throw new InvalidExpressionCustomException("Invalid syntax in expression");

        return valStack.Pop();
    }

    private static double ToRadians(double val, AngleMode mode) => mode switch
    {
        AngleMode.Degrees => val * (Math.PI / 180.0),
        AngleMode.Gradians => val * (Math.PI / 200.0),
        _ => val // Radians
    };

    private static double FromRadians(double rad, AngleMode mode) => mode switch
    {
        AngleMode.Degrees => rad * (180.0 / Math.PI),
        AngleMode.Gradians => rad * (200.0 / Math.PI),
        _ => rad // Radians
    };

    private static double EvaluateFunction(string func, double arg, AngleMode angleMode)
    {
        return func switch
        {
            "sin" => Math.Sin(ToRadians(arg, angleMode)),
            "cos" => Math.Cos(ToRadians(arg, angleMode)),
            "tan" => Math.Tan(ToRadians(arg, angleMode)),
            "asin" => FromRadians(Math.Asin(arg), angleMode),
            "acos" => FromRadians(Math.Acos(arg), angleMode),
            "atan" => FromRadians(Math.Atan(arg), angleMode),
            "sinh" => Math.Sinh(arg),
            "cosh" => Math.Cosh(arg),
            "tanh" => Math.Tanh(arg),
            "asinh" => Math.Asinh(arg),
            "acosh" => Math.Acosh(arg),
            "atanh" => Math.Atanh(arg),
            "log10" or "log" => arg <= 0 ? throw new InvalidExpressionCustomException("Logarithm requires positive number") : Math.Log10(arg),
            "ln" => arg <= 0 ? throw new InvalidExpressionCustomException("Natural log requires positive number") : Math.Log(arg),
            "exp" => Math.Exp(arg),
            "sqrt" => arg < 0 ? throw new InvalidExpressionCustomException("Square root requires non-negative number") : Math.Sqrt(arg),
            "cbrt" => Math.Cbrt(arg),
            "abs" => Math.Abs(arg),
            "floor" => Math.Floor(arg),
            "ceil" => Math.Ceiling(arg),
            "round" => Math.Round(arg),
            "fact" => Factorial(arg),
            "deg" => arg * (180.0 / Math.PI),
            "rad" => arg * (Math.PI / 180.0),
            "grad" => arg * (200.0 / Math.PI),
            _ => throw new InvalidExpressionCustomException($"Unknown function '{func}'")
        };
    }

    private static double Factorial(double n)
    {
        if (n < 0 || Math.Floor(n) != n)
            throw new InvalidExpressionCustomException("Factorial is only defined for non-negative integers");
        if (n > 170)
            throw new MathOverflowCustomException();

        double res = 1;
        for (int i = 2; i <= (int)n; i++)
        {
            res *= i;
        }
        return res;
    }
}
