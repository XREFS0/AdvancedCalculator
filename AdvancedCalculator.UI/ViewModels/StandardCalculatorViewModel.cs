using System.Globalization;
using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class StandardCalculatorViewModel : ViewModelBase
{
    private readonly IExpressionEngine _engine;
    private readonly IHistoryRepository _historyRepository;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsRepository _settingsRepository;

    private string _displayExpression = "";
    private string _currentInput = "0";
    private string _statusMessage = "";
    private bool _isNewCalculation = true;
    private double _memoryValue = 0;
    private bool _hasMemory = false;

    public string DisplayExpression
    {
        get => _displayExpression;
        set => SetProperty(ref _displayExpression, value);
    }

    public string CurrentInput
    {
        get => _currentInput;
        set => SetProperty(ref _currentInput, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool HasMemory
    {
        get => _hasMemory;
        set => SetProperty(ref _hasMemory, value);
    }

    public ICommand AppendDigitCommand { get; }
    public ICommand AppendOperatorCommand { get; }
    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand NegateCommand { get; }
    public ICommand PercentageCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand ParenthesisCommand { get; }
    public ICommand QuickFunctionCommand { get; }
    public ICommand MemoryCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }

    public StandardCalculatorViewModel(
        IExpressionEngine engine,
        IHistoryRepository historyRepository,
        IClipboardService clipboardService,
        ISettingsRepository settingsRepository)
    {
        _engine = engine;
        _historyRepository = historyRepository;
        _clipboardService = clipboardService;
        _settingsRepository = settingsRepository;

        AppendDigitCommand = new RelayCommand(param => AppendDigit(param?.ToString() ?? ""));
        AppendOperatorCommand = new RelayCommand(param => AppendOperator(param?.ToString() ?? ""));
        CalculateCommand = new RelayCommand(Calculate);
        ClearCommand = new RelayCommand(Clear);
        ClearEntryCommand = new RelayCommand(ClearEntry);
        BackspaceCommand = new RelayCommand(Backspace);
        NegateCommand = new RelayCommand(Negate);
        PercentageCommand = new RelayCommand(Percentage);
        DecimalCommand = new RelayCommand(AppendDecimal);
        ParenthesisCommand = new RelayCommand(param => AppendParenthesis(param?.ToString() ?? "("));
        QuickFunctionCommand = new RelayCommand(param => ExecuteQuickFunction(param?.ToString() ?? ""));
        MemoryCommand = new RelayCommand(param => ExecuteMemoryAction(param?.ToString() ?? ""));
        CopyCommand = new RelayCommand(CopyResult);
        PasteCommand = new RelayCommand(PasteInput);
    }

    public void AppendDigit(string digit)
    {
        StatusMessage = "";
        if (_isNewCalculation || CurrentInput == "0")
        {
            CurrentInput = digit;
            _isNewCalculation = false;
        }
        else
        {
            CurrentInput += digit;
        }
    }

    public void AppendDecimal()
    {
        StatusMessage = "";
        if (_isNewCalculation)
        {
            CurrentInput = "0.";
            _isNewCalculation = false;
        }
        else if (!CurrentInput.Contains('.'))
        {
            CurrentInput += ".";
        }
    }

    public void AppendOperator(string op)
    {
        StatusMessage = "";
        DisplayExpression = $"{CurrentInput} {op}";
        CurrentInput = "0";
        _isNewCalculation = true;
    }

    public void AppendParenthesis(string paren)
    {
        StatusMessage = "";
        if (paren == "(")
        {
            DisplayExpression += " (";
        }
        else
        {
            DisplayExpression += " )";
        }
    }

    public void Calculate()
    {
        string expr = string.IsNullOrWhiteSpace(DisplayExpression)
            ? CurrentInput
            : $"{DisplayExpression} {CurrentInput}";

        if (_engine.TryEvaluate(expr, out double res, out string error, AngleMode.Degrees))
        {
            var settings = _settingsRepository.LoadSettings();
            string formatted = Math.Round(res, settings.DecimalPrecision).ToString(CultureInfo.InvariantCulture);
            
            if (settings.EnableHistorySaving)
            {
                _historyRepository.AddAsync(new CalculationRecord
                {
                    Expression = expr,
                    Result = formatted,
                    Mode = CalculatorMode.Standard,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            if (settings.AutoCopyResult)
            {
                _clipboardService.SetText(formatted);
            }

            DisplayExpression = $"{expr} =";
            CurrentInput = formatted;
            _isNewCalculation = true;
            StatusMessage = "";
        }
        else
        {
            StatusMessage = error;
        }
    }

    public void Clear()
    {
        DisplayExpression = "";
        CurrentInput = "0";
        StatusMessage = "";
        _isNewCalculation = true;
    }

    public void ClearEntry()
    {
        CurrentInput = "0";
        StatusMessage = "";
        _isNewCalculation = true;
    }

    public void Backspace()
    {
        StatusMessage = "";
        if (CurrentInput.Length > 1)
        {
            CurrentInput = CurrentInput[..^1];
        }
        else
        {
            CurrentInput = "0";
            _isNewCalculation = true;
        }
    }

    public void Negate()
    {
        if (double.TryParse(CurrentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
        {
            CurrentInput = (-val).ToString(CultureInfo.InvariantCulture);
        }
    }

    public void Percentage()
    {
        if (double.TryParse(CurrentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
        {
            CurrentInput = (val / 100.0).ToString(CultureInfo.InvariantCulture);
        }
    }

    public void ExecuteQuickFunction(string func)
    {
        if (!double.TryParse(CurrentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            return;

        double res = func switch
        {
            "sqrt" => val < 0 ? double.NaN : Math.Sqrt(val),
            "sqr" => val * val,
            "recip" => val == 0 ? double.NaN : 1.0 / val,
            _ => val
        };

        if (double.IsNaN(res))
        {
            StatusMessage = "Invalid operation";
        }
        else
        {
            CurrentInput = res.ToString(CultureInfo.InvariantCulture);
            _isNewCalculation = true;
        }
    }

    public void ExecuteMemoryAction(string action)
    {
        double.TryParse(CurrentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double val);

        switch (action.ToUpperInvariant())
        {
            case "MC":
                _memoryValue = 0;
                HasMemory = false;
                break;
            case "MR":
                CurrentInput = _memoryValue.ToString(CultureInfo.InvariantCulture);
                _isNewCalculation = true;
                break;
            case "M+":
                _memoryValue += val;
                HasMemory = true;
                _isNewCalculation = true;
                break;
            case "M-":
                _memoryValue -= val;
                HasMemory = true;
                _isNewCalculation = true;
                break;
            case "MS":
                _memoryValue = val;
                HasMemory = true;
                _isNewCalculation = true;
                break;
        }
    }

    public void CopyResult()
    {
        _clipboardService.SetText(CurrentInput);
        StatusMessage = "Copied to clipboard!";
    }

    public void PasteInput()
    {
        string text = _clipboardService.GetText();
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            CurrentInput = text;
            _isNewCalculation = false;
            StatusMessage = "";
        }
    }

    public void LoadFromHistory(CalculationRecord record)
    {
        DisplayExpression = record.Expression;
        CurrentInput = record.Result;
        _isNewCalculation = true;
    }
}
