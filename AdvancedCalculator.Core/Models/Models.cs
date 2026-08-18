using AdvancedCalculator.Core.Enums;

namespace AdvancedCalculator.Core.Models;

public class CalculationRecord
{
    public long Id { get; set; }
    public string Expression { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public CalculatorMode Mode { get; set; } = CalculatorMode.Standard;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsPinned { get; set; }
}

public class MemoryItem
{
    public double Value { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class CurrencyRate
{
    public string CurrencyCode { get; set; } = string.Empty; // e.g. USD, EUR, SAR
    public string CurrencyName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal RateToBaseUSD { get; set; } // Rate compared to 1 USD
}

public class UnitItem
{
    public string Name { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public UnitCategory Category { get; set; }
    public double FactorToBase { get; set; } // Conversion multiplier to base unit
    public double Offset { get; set; } = 0; // For temperature (e.g. Kelvin/Fahrenheit)
}

public class AppSettings
{
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string LanguageCode { get; set; } = "en-US";
    public CalculatorMode DefaultMode { get; set; } = CalculatorMode.Standard;
    public int DecimalPrecision { get; set; } = 10;
    public bool AutoCopyResult { get; set; } = false;
    public bool AlwaysOnTop { get; set; } = false;
    public bool EnableHistorySaving { get; set; } = true;
    public int MaxHistoryCount { get; set; } = 100;
}
