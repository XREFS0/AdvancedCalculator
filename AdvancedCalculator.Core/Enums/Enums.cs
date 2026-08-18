namespace AdvancedCalculator.Core.Enums;

public enum CalculatorMode
{
    Standard,
    Scientific,
    Programmer,
    UnitConverter,
    CurrencyConverter,
    History,
    Settings,
    About
}

public enum AngleMode
{
    Degrees,
    Radians,
    Gradians
}

public enum NumberBase
{
    Hexadecimal = 16,
    Decimal = 10,
    Octal = 8,
    Binary = 2
}

public enum BitSize
{
    Qword = 64, // long / ulong
    Dword = 32, // int / uint
    Word = 16,  // short / ushort
    Byte = 8    // sbyte / byte
}

public enum AppTheme
{
    Dark,
    Light,
    System
}

public enum UnitCategory
{
    Length,
    Weight,
    Temperature,
    Area,
    Volume,
    Speed,
    Time,
    DataStorage,
    Energy,
    Pressure
}
