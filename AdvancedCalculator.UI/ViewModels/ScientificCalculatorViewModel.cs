using System.Globalization;
using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class ScientificCalculatorViewModel : ViewModelBase
{
    private readonly IExpressionEngine _engine;
    private readonly IHistoryRepository _historyRepository;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsRepository _settingsRepository;

    private string _displayExpression = "";
    private string _currentInput = "0";
    private string _statusMessage = "";
    private bool _isNewCalculation = true;
    private AngleMode _angleMode = AngleMode.Degrees;
    private bool _is2ndFunction = false;

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

    public AngleMode CurrentAngleMode
    {
        get => _angleMode;
        set => SetProperty(ref _angleMode, value);
    }

    public bool Is2ndFunction
    {
        get => _is2ndFunction;
        set => SetProperty(ref _is2ndFunction, value);
    }

    public ICommand AppendDigitCommand { get; }
    public ICommand AppendOperatorCommand { get; }
    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand NegateCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand ParenthesisCommand { get; }
    public ICommand ScientificFunctionCommand { get; }
    public ICommand InsertConstantCommand { get; }
    public ICommand ToggleAngleModeCommand { get; }
    public ICommand Toggle2ndCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }

    public ScientificCalculatorViewModel(
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
        DecimalCommand = new RelayCommand(AppendDecimal);
        ParenthesisCommand = new RelayCommand(param => AppendParenthesis(param?.ToString() ?? "("));
        ScientificFunctionCommand = new RelayCommand(param => ApplyFunction(param?.ToString() ?? ""));
        InsertConstantCommand = new RelayCommand(param => InsertConstant(param?.ToString() ?? "pi"));
        ToggleAngleModeCommand = new RelayCommand(ToggleAngleMode);
        Toggle2ndCommand = new RelayCommand(() => Is2ndFunction = !Is2ndFunction);
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
            DisplayExpression += " (";
        else
            DisplayExpression += " )";
    }

    public void InsertConstant(string constant)
    {
        StatusMessage = "";
        double val = constant.ToLowerInvariant() switch
        {
            "pi" or "π" => Math.PI,
            "e" => Math.E,
            _ => 0
        };
        CurrentInput = val.ToString(CultureInfo.InvariantCulture);
        _isNewCalculation = true;
    }

    public void ToggleAngleMode()
    {
        CurrentAngleMode = CurrentAngleMode switch
        {
            AngleMode.Degrees => AngleMode.Radians,
            AngleMode.Radians => AngleMode.Gradians,
            AngleMode.Gradians => AngleMode.Degrees,
            _ => AngleMode.Degrees
        };
    }

    public void ApplyFunction(string func)
    {
        StatusMessage = "";
        string actualFunc = func;
        if (Is2ndFunction)
        {
            actualFunc = func switch
            {
                "sin" => "asin",
                "cos" => "acos",
                "tan" => "atan",
                "sinh" => "asinh",
                "cosh" => "acosh",
                "tanh" => "atanh",
                "ln" => "exp",
                "log10" => "10^x",
                _ => func
            };
        }

        if (actualFunc == "10^x")
        {
            if (double.TryParse(CurrentInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
            {
                CurrentInput = Math.Pow(10, p).ToString(CultureInfo.InvariantCulture);
                _isNewCalculation = true;
            }
            return;
        }

        string expr = $"{actualFunc}({CurrentInput})";
        if (_engine.TryEvaluate(expr, out double res, out string error, CurrentAngleMode))
        {
            var settings = _settingsRepository.LoadSettings();
            string formatted = Math.Round(res, settings.DecimalPrecision).ToString(CultureInfo.InvariantCulture);
            CurrentInput = formatted;
            _isNewCalculation = true;
        }
        else
        {
            StatusMessage = error;
        }
    }

    public void Calculate()
    {
        string expr = string.IsNullOrWhiteSpace(DisplayExpression)
            ? CurrentInput
            : $"{DisplayExpression} {CurrentInput}";

        if (_engine.TryEvaluate(expr, out double res, out string error, CurrentAngleMode))
        {
            var settings = _settingsRepository.LoadSettings();
            string formatted = Math.Round(res, settings.DecimalPrecision).ToString(CultureInfo.InvariantCulture);

            if (settings.EnableHistorySaving)
            {
                _historyRepository.AddAsync(new CalculationRecord
                {
                    Expression = expr,
                    Result = formatted,
                    Mode = CalculatorMode.Scientific,
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
}
