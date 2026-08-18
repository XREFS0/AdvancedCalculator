using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class UnitConverterViewModel : ViewModelBase
{
    private readonly IUnitConverterService _unitService;
    private readonly IClipboardService _clipboardService;

    private UnitCategory _selectedCategory;
    private UnitItem? _fromUnit;
    private UnitItem? _toUnit;
    private string _fromValue = "1";
    private string _toValue = "0";
    private string _formulaText = "";

    public ObservableCollection<UnitCategory> Categories { get; } = new();
    public ObservableCollection<UnitItem> AvailableUnits { get; } = new();

    public UnitCategory SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                LoadUnitsForCategory();
            }
        }
    }

    public UnitItem? FromUnit
    {
        get => _fromUnit;
        set
        {
            if (SetProperty(ref _fromUnit, value))
            {
                Recalculate();
            }
        }
    }

    public UnitItem? ToUnit
    {
        get => _toUnit;
        set
        {
            if (SetProperty(ref _toUnit, value))
            {
                Recalculate();
            }
        }
    }

    public string FromValue
    {
        get => _fromValue;
        set
        {
            if (SetProperty(ref _fromValue, value))
            {
                Recalculate();
            }
        }
    }

    public string ToValue
    {
        get => _toValue;
        set => SetProperty(ref _toValue, value);
    }

    public string FormulaText
    {
        get => _formulaText;
        set => SetProperty(ref _formulaText, value);
    }

    public ICommand SwapUnitsCommand { get; }
    public ICommand AppendDigitCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand CopyCommand { get; }

    public UnitConverterViewModel(IUnitConverterService unitService, IClipboardService clipboardService)
    {
        _unitService = unitService;
        _clipboardService = clipboardService;

        SwapUnitsCommand = new RelayCommand(SwapUnits);
        AppendDigitCommand = new RelayCommand(param => AppendDigit(param?.ToString() ?? ""));
        ClearCommand = new RelayCommand(() => FromValue = "0");
        BackspaceCommand = new RelayCommand(Backspace);
        CopyCommand = new RelayCommand(() => _clipboardService.SetText(ToValue));

        foreach (var cat in _unitService.GetCategories())
        {
            Categories.Add(cat);
        }

        if (Categories.Count > 0)
        {
            SelectedCategory = Categories[0];
        }
    }

    private void LoadUnitsForCategory()
    {
        AvailableUnits.Clear();
        var units = _unitService.GetUnits(SelectedCategory);
        foreach (var u in units)
        {
            AvailableUnits.Add(u);
        }

        if (AvailableUnits.Count > 1)
        {
            FromUnit = AvailableUnits[0];
            ToUnit = AvailableUnits[1];
        }
        else if (AvailableUnits.Count > 0)
        {
            FromUnit = AvailableUnits[0];
            ToUnit = AvailableUnits[0];
        }
    }

    public void AppendDigit(string digit)
    {
        if (FromValue == "0" && digit != ".")
        {
            FromValue = digit;
        }
        else if (digit == "." && FromValue.Contains('.'))
        {
            // Ignore extra dot
        }
        else
        {
            FromValue += digit;
        }
    }

    public void Backspace()
    {
        if (FromValue.Length > 1)
        {
            FromValue = FromValue[..^1];
        }
        else
        {
            FromValue = "0";
        }
    }

    private void SwapUnits()
    {
        var temp = FromUnit;
        FromUnit = ToUnit;
        ToUnit = temp;
    }

    private void Recalculate()
    {
        if (FromUnit == null || ToUnit == null) return;

        if (double.TryParse(FromValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
        {
            double res = _unitService.Convert(val, FromUnit, ToUnit);
            ToValue = Math.Round(res, 8).ToString(CultureInfo.InvariantCulture);
            FormulaText = $"1 {FromUnit.Symbol} = {_unitService.Convert(1, FromUnit, ToUnit):G6} {ToUnit.Symbol}";
        }
        else
        {
            ToValue = "0";
            FormulaText = "";
        }
    }
}
