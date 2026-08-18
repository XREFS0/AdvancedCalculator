using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Core.Interfaces;

public interface IExpressionEngine
{
    double Evaluate(string expression, AngleMode angleMode = AngleMode.Degrees);
    bool TryEvaluate(string expression, out double result, out string errorMessage, AngleMode angleMode = AngleMode.Degrees);
}

public interface IProgrammerService
{
    long EvaluateBitwise(long left, string op, long right, BitSize bitSize);
    long PerformUnaryOp(string op, long value, BitSize bitSize);
    long ShiftLeft(long value, int count, BitSize bitSize);
    long ShiftRight(long value, int count, BitSize bitSize);
    long MaskToBitSize(long value, BitSize bitSize);
    string FormatNumber(long value, NumberBase numberBase, BitSize bitSize);
    bool TryParse(string input, NumberBase numberBase, BitSize bitSize, out long value);
}

public interface IUnitConverterService
{
    IReadOnlyList<UnitCategory> GetCategories();
    IReadOnlyList<UnitItem> GetUnits(UnitCategory category);
    double Convert(double value, UnitItem fromUnit, UnitItem toUnit);
}

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyRate>> GetRatesAsync();
    decimal Convert(decimal amount, string fromCode, string toCode, IReadOnlyList<CurrencyRate> rates);
    Task<DateTime> GetLastUpdatedTimeAsync();
}

public interface IHistoryRepository
{
    Task<IReadOnlyList<CalculationRecord>> GetAllAsync();
    Task<IReadOnlyList<CalculationRecord>> SearchAsync(string query);
    Task<CalculationRecord> AddAsync(CalculationRecord record);
    Task<bool> DeleteAsync(long id);
    Task ClearAllAsync();
    Task TogglePinAsync(long id, bool isPinned);
}

public interface ISettingsRepository
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
}

public interface IClipboardService
{
    void SetText(string text);
    string GetText();
}
