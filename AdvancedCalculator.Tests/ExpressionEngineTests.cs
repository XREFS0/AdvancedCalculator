using AdvancedCalculator.Application.ExpressionEngine;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace AdvancedCalculator.Tests.ExpressionEngineTests;

public class ExpressionEngineTests
{
    private readonly ExpressionEngine _engine = new();

    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("3 * 7", 21)]
    [InlineData("20 / 4", 5)]
    [InlineData("2 + 3 * 4", 14)] // Precedence
    [InlineData("(2 + 3) * 4", 20)] // Parentheses
    [InlineData("2 ^ 3", 8)]
    [InlineData("2 ^ 3 ^ 2", 512)] // Right associative: 2^(3^2) = 2^9 = 512
    [InlineData("10 % 3", 1)]
    public void BasicArithmetic_ShouldEvaluateCorrectly(string expression, double expected)
    {
        double result = _engine.Evaluate(expression);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData("-5 + 10", 5)]
    [InlineData("10 * -2", -20)]
    [InlineData("-(-5)", 5)]
    [InlineData("2 * (-3 + 5)", 4)]
    public void UnaryMinus_ShouldEvaluateCorrectly(string expression, double expected)
    {
        double result = _engine.Evaluate(expression);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData("sin(90)", 1.0, AngleMode.Degrees)]
    [InlineData("cos(0)", 1.0, AngleMode.Degrees)]
    [InlineData("tan(45)", 1.0, AngleMode.Degrees)]
    [InlineData("sqrt(16)", 4.0, AngleMode.Degrees)]
    [InlineData("cbrt(27)", 3.0, AngleMode.Degrees)]
    [InlineData("abs(-42)", 42.0, AngleMode.Degrees)]
    [InlineData("fact(5)", 120.0, AngleMode.Degrees)]
    [InlineData("5!", 120.0, AngleMode.Degrees)]
    [InlineData("log10(100)", 2.0, AngleMode.Degrees)]
    public void ScientificFunctions_ShouldEvaluateCorrectly(string expression, double expected, AngleMode mode)
    {
        double result = _engine.Evaluate(expression, mode);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Theory]
    [InlineData("2(3)", 6)]
    [InlineData("(2 + 3)(4)", 20)]
    [InlineData("2sqrt(9)", 6)]
    public void ImplicitMultiplication_ShouldEvaluateCorrectly(string expression, double expected)
    {
        double result = _engine.Evaluate(expression);
        result.Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void DivisionByZero_ShouldThrowOrHandleSafely()
    {
        bool success = _engine.TryEvaluate("10 / 0", out _, out string error);
        success.Should().BeFalse();
        error.Should().Contain("divide by zero");
    }

    [Fact]
    public void MismatchedParentheses_ShouldReturnError()
    {
        bool success = _engine.TryEvaluate("(2 + 3 * 4", out _, out string error);
        success.Should().BeFalse();
    }
}
