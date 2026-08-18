using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Application.Services;

public class UnitConverterService : IUnitConverterService
{
    private static readonly Dictionary<UnitCategory, List<UnitItem>> CategoryUnits = new()
    {
        {
            UnitCategory.Length, new()
            {
                new() { Name = "Meter", PluralName = "Meters", Symbol = "m", Category = UnitCategory.Length, FactorToBase = 1.0 },
                new() { Name = "Kilometer", PluralName = "Kilometers", Symbol = "km", Category = UnitCategory.Length, FactorToBase = 1000.0 },
                new() { Name = "Centimeter", PluralName = "Centimeters", Symbol = "cm", Category = UnitCategory.Length, FactorToBase = 0.01 },
                new() { Name = "Millimeter", PluralName = "Millimeters", Symbol = "mm", Category = UnitCategory.Length, FactorToBase = 0.001 },
                new() { Name = "Mile", PluralName = "Miles", Symbol = "mi", Category = UnitCategory.Length, FactorToBase = 1609.344 },
                new() { Name = "Yard", PluralName = "Yards", Symbol = "yd", Category = UnitCategory.Length, FactorToBase = 0.9144 },
                new() { Name = "Foot", PluralName = "Feet", Symbol = "ft", Category = UnitCategory.Length, FactorToBase = 0.3048 },
                new() { Name = "Inch", PluralName = "Inches", Symbol = "in", Category = UnitCategory.Length, FactorToBase = 0.0254 },
                new() { Name = "Nautical Mile", PluralName = "Nautical Miles", Symbol = "nmi", Category = UnitCategory.Length, FactorToBase = 1852.0 }
            }
        },
        {
            UnitCategory.Weight, new()
            {
                new() { Name = "Kilogram", PluralName = "Kilograms", Symbol = "kg", Category = UnitCategory.Weight, FactorToBase = 1.0 },
                new() { Name = "Gram", PluralName = "Grams", Symbol = "g", Category = UnitCategory.Weight, FactorToBase = 0.001 },
                new() { Name = "Milligram", PluralName = "Milligrams", Symbol = "mg", Category = UnitCategory.Weight, FactorToBase = 1e-6 },
                new() { Name = "Metric Ton", PluralName = "Metric Tons", Symbol = "t", Category = UnitCategory.Weight, FactorToBase = 1000.0 },
                new() { Name = "Pound", PluralName = "Pounds", Symbol = "lb", Category = UnitCategory.Weight, FactorToBase = 0.45359237 },
                new() { Name = "Ounce", PluralName = "Ounces", Symbol = "oz", Category = UnitCategory.Weight, FactorToBase = 0.028349523125 },
                new() { Name = "Stone", PluralName = "Stones", Symbol = "st", Category = UnitCategory.Weight, FactorToBase = 6.35029318 }
            }
        },
        {
            UnitCategory.Temperature, new()
            {
                new() { Name = "Celsius", PluralName = "Celsius", Symbol = "°C", Category = UnitCategory.Temperature, FactorToBase = 1.0, Offset = 0 },
                new() { Name = "Fahrenheit", PluralName = "Fahrenheit", Symbol = "°F", Category = UnitCategory.Temperature, FactorToBase = 5.0 / 9.0, Offset = 32 },
                new() { Name = "Kelvin", PluralName = "Kelvin", Symbol = "K", Category = UnitCategory.Temperature, FactorToBase = 1.0, Offset = 273.15 }
            }
        },
        {
            UnitCategory.Area, new()
            {
                new() { Name = "Square Meter", PluralName = "Square Meters", Symbol = "m²", Category = UnitCategory.Area, FactorToBase = 1.0 },
                new() { Name = "Square Kilometer", PluralName = "Square Kilometers", Symbol = "km²", Category = UnitCategory.Area, FactorToBase = 1e6 },
                new() { Name = "Hectare", PluralName = "Hectares", Symbol = "ha", Category = UnitCategory.Area, FactorToBase = 10000.0 },
                new() { Name = "Acre", PluralName = "Acres", Symbol = "ac", Category = UnitCategory.Area, FactorToBase = 4046.8564224 },
                new() { Name = "Square Foot", PluralName = "Square Feet", Symbol = "ft²", Category = UnitCategory.Area, FactorToBase = 0.09290304 },
                new() { Name = "Square Mile", PluralName = "Square Miles", Symbol = "mi²", Category = UnitCategory.Area, FactorToBase = 2589988.110336 }
            }
        },
        {
            UnitCategory.Volume, new()
            {
                new() { Name = "Liter", PluralName = "Liters", Symbol = "L", Category = UnitCategory.Volume, FactorToBase = 1.0 },
                new() { Name = "Milliliter", PluralName = "Milliliters", Symbol = "mL", Category = UnitCategory.Volume, FactorToBase = 0.001 },
                new() { Name = "Cubic Meter", PluralName = "Cubic Meters", Symbol = "m³", Category = UnitCategory.Volume, FactorToBase = 1000.0 },
                new() { Name = "US Gallon", PluralName = "US Gallons", Symbol = "gal", Category = UnitCategory.Volume, FactorToBase = 3.785411784 },
                new() { Name = "US Quart", PluralName = "US Quarts", Symbol = "qt", Category = UnitCategory.Volume, FactorToBase = 0.946352946 },
                new() { Name = "US Pint", PluralName = "US Pints", Symbol = "pt", Category = UnitCategory.Volume, FactorToBase = 0.473176473 },
                new() { Name = "US Cup", PluralName = "US Cups", Symbol = "cup", Category = UnitCategory.Volume, FactorToBase = 0.2365882365 }
            }
        },
        {
            UnitCategory.Speed, new()
            {
                new() { Name = "Meters per second", PluralName = "m/s", Symbol = "m/s", Category = UnitCategory.Speed, FactorToBase = 1.0 },
                new() { Name = "Kilometers per hour", PluralName = "km/h", Symbol = "km/h", Category = UnitCategory.Speed, FactorToBase = 1.0 / 3.6 },
                new() { Name = "Miles per hour", PluralName = "mph", Symbol = "mph", Category = UnitCategory.Speed, FactorToBase = 0.44704 },
                new() { Name = "Knot", PluralName = "Knots", Symbol = "kn", Category = UnitCategory.Speed, FactorToBase = 0.514444 }
            }
        },
        {
            UnitCategory.Time, new()
            {
                new() { Name = "Second", PluralName = "Seconds", Symbol = "s", Category = UnitCategory.Time, FactorToBase = 1.0 },
                new() { Name = "Minute", PluralName = "Minutes", Symbol = "min", Category = UnitCategory.Time, FactorToBase = 60.0 },
                new() { Name = "Hour", PluralName = "Hours", Symbol = "h", Category = UnitCategory.Time, FactorToBase = 3600.0 },
                new() { Name = "Day", PluralName = "Days", Symbol = "d", Category = UnitCategory.Time, FactorToBase = 86400.0 },
                new() { Name = "Week", PluralName = "Weeks", Symbol = "wk", Category = UnitCategory.Time, FactorToBase = 604800.0 },
                new() { Name = "Year", PluralName = "Years", Symbol = "yr", Category = UnitCategory.Time, FactorToBase = 31536000.0 }
            }
        },
        {
            UnitCategory.DataStorage, new()
            {
                new() { Name = "Byte", PluralName = "Bytes", Symbol = "B", Category = UnitCategory.DataStorage, FactorToBase = 1.0 },
                new() { Name = "Kilobyte", PluralName = "Kilobytes", Symbol = "KB", Category = UnitCategory.DataStorage, FactorToBase = 1024.0 },
                new() { Name = "Megabyte", PluralName = "Megabytes", Symbol = "MB", Category = UnitCategory.DataStorage, FactorToBase = 1024.0 * 1024 },
                new() { Name = "Gigabyte", PluralName = "Gigabytes", Symbol = "GB", Category = UnitCategory.DataStorage, FactorToBase = 1024.0 * 1024 * 1024 },
                new() { Name = "Terabyte", PluralName = "Terabytes", Symbol = "TB", Category = UnitCategory.DataStorage, FactorToBase = Math.Pow(1024, 4) },
                new() { Name = "Bit", PluralName = "Bits", Symbol = "bit", Category = UnitCategory.DataStorage, FactorToBase = 0.125 }
            }
        },
        {
            UnitCategory.Energy, new()
            {
                new() { Name = "Joule", PluralName = "Joules", Symbol = "J", Category = UnitCategory.Energy, FactorToBase = 1.0 },
                new() { Name = "Kilojoule", PluralName = "Kilojoules", Symbol = "kJ", Category = UnitCategory.Energy, FactorToBase = 1000.0 },
                new() { Name = "Calorie", PluralName = "Calories", Symbol = "cal", Category = UnitCategory.Energy, FactorToBase = 4.184 },
                new() { Name = "Kilocalorie", PluralName = "Kilocalories", Symbol = "kcal", Category = UnitCategory.Energy, FactorToBase = 4184.0 },
                new() { Name = "Watt-hour", PluralName = "Watt-hours", Symbol = "Wh", Category = UnitCategory.Energy, FactorToBase = 3600.0 },
                new() { Name = "Kilowatt-hour", PluralName = "Kilowatt-hours", Symbol = "kWh", Category = UnitCategory.Energy, FactorToBase = 3600000.0 },
                new() { Name = "Electronvolt", PluralName = "Electronvolts", Symbol = "eV", Category = UnitCategory.Energy, FactorToBase = 1.602176634e-19 }
            }
        },
        {
            UnitCategory.Pressure, new()
            {
                new() { Name = "Pascal", PluralName = "Pascals", Symbol = "Pa", Category = UnitCategory.Pressure, FactorToBase = 1.0 },
                new() { Name = "Kilopascal", PluralName = "Kilopascals", Symbol = "kPa", Category = UnitCategory.Pressure, FactorToBase = 1000.0 },
                new() { Name = "Bar", PluralName = "Bars", Symbol = "bar", Category = UnitCategory.Pressure, FactorToBase = 100000.0 },
                new() { Name = "Atmosphere", PluralName = "Atmospheres", Symbol = "atm", Category = UnitCategory.Pressure, FactorToBase = 101325.0 },
                new() { Name = "Pound per sq inch", PluralName = "PSI", Symbol = "psi", Category = UnitCategory.Pressure, FactorToBase = 6894.757 }
            }
        }
    };

    public IReadOnlyList<UnitCategory> GetCategories() => CategoryUnits.Keys.ToList();

    public IReadOnlyList<UnitItem> GetUnits(UnitCategory category) => CategoryUnits.TryGetValue(category, out var units) ? units : new List<UnitItem>();

    public double Convert(double value, UnitItem fromUnit, UnitItem toUnit)
    {
        if (fromUnit == toUnit) return value;

        if (fromUnit.Category == UnitCategory.Temperature)
        {
            // First convert fromUnit to Celsius base
            double inCelsius = fromUnit.Name switch
            {
                "Celsius" => value,
                "Fahrenheit" => (value - 32.0) * (5.0 / 9.0),
                "Kelvin" => value - 273.15,
                _ => value
            };

            // Then convert from Celsius to toUnit
            return toUnit.Name switch
            {
                "Celsius" => inCelsius,
                "Fahrenheit" => (inCelsius * (9.0 / 5.0)) + 32.0,
                "Kelvin" => inCelsius + 273.15,
                _ => inCelsius
            };
        }

        // Standard linear conversion
        double baseValue = value * fromUnit.FactorToBase;
        return baseValue / toUnit.FactorToBase;
    }
}
