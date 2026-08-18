using AdvancedCalculator.Application.Services;
using AdvancedCalculator.Core.Enums;
using FluentAssertions;
using Xunit;

namespace AdvancedCalculator.Tests;

public class ProgrammerAndUnitTests
{
    private readonly ProgrammerService _programmerService = new();
    private readonly UnitConverterService _unitService = new();

    [Theory]
    [InlineData(12, "AND", 10, BitSize.Byte, 8)]
    [InlineData(12, "OR", 10, BitSize.Byte, 14)]
    [InlineData(12, "XOR", 10, BitSize.Byte, 6)]
    public void BitwiseOperations_ShouldCalculateCorrectly(long left, string op, long right, BitSize size, long expected)
    {
        long res = _programmerService.EvaluateBitwise(left, op, right, size);
        res.Should().Be(expected);
    }

    [Fact]
    public void UnitConverter_LengthConversion_ShouldBeAccurate()
    {
        var units = _unitService.GetUnits(UnitCategory.Length);
        var km = units.First(u => u.Symbol == "km");
        var m = units.First(u => u.Symbol == "m");

        double result = _unitService.Convert(5.5, km, m);
        result.Should().Be(5500);
    }

    [Fact]
    public void UnitConverter_TemperatureCelsiusToFahrenheit_ShouldBeAccurate()
    {
        var units = _unitService.GetUnits(UnitCategory.Temperature);
        var c = units.First(u => u.Symbol == "°C");
        var f = units.First(u => u.Symbol == "°F");

        double result = _unitService.Convert(100, c, f);
        result.Should().Be(212);
    }
}
