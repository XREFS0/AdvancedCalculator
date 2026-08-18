using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class ProgrammerCalculatorViewModel : ViewModelBase
{
    private readonly IProgrammerService _programmerService;
    private readonly IHistoryRepository _historyRepository;
    private readonly IClipboardService _clipboardService;
    private readonly ISettingsRepository _settingsRepository;

    private long _currentValue = 0;
    private long? _pendingValue = null;
    private string _pendingOp = "";
    private NumberBase _activeBase = NumberBase.Decimal;
    private BitSize _activeBitSize = BitSize.Qword;
    private string _currentInput = "0";
    private string _statusMessage = "";
    private bool _isNewEntry = true;

    public NumberBase ActiveBase
    {
        get => _activeBase;
        set
        {
            if (SetProperty(ref _activeBase, value))
            {
                UpdateAllRepresentations();
                CurrentInput = _programmerService.FormatNumber(_currentValue, _activeBase, _activeBitSize);
            }
        }
    }

    public BitSize ActiveBitSize
    {
        get => _activeBitSize;
        set
        {
            if (SetProperty(ref _activeBitSize, value))
            {
                _currentValue = _programmerService.MaskToBitSize(_currentValue, _activeBitSize);
                UpdateAllRepresentations();
                CurrentInput = _programmerService.FormatNumber(_currentValue, _activeBase, _activeBitSize);
            }
        }
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

    private string _hexDisplay = "0";
    public string HexDisplay { get => _hexDisplay; set => SetProperty(ref _hexDisplay, value); }

    private string _decDisplay = "0";
    public string DecDisplay { get => _decDisplay; set => SetProperty(ref _decDisplay, value); }

    private string _octDisplay = "0";
    public string OctDisplay { get => _octDisplay; set => SetProperty(ref _octDisplay, value); }

    private string _binDisplay = "0000 0000";
    public string BinDisplay { get => _binDisplay; set => SetProperty(ref _binDisplay, value); }

    public ICommand AppendCharCommand { get; }
    public ICommand OperatorCommand { get; }
    public ICommand UnaryOpCommand { get; }
    public ICommand ShiftLeftCommand { get; }
    public ICommand ShiftRightCommand { get; }
    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand SwitchBaseCommand { get; }
    public ICommand SwitchBitSizeCommand { get; }
    public ICommand CopyCommand { get; }

    public ProgrammerCalculatorViewModel(
        IProgrammerService programmerService,
        IHistoryRepository historyRepository,
        IClipboardService clipboardService,
        ISettingsRepository settingsRepository)
    {
        _programmerService = programmerService;
        _historyRepository = historyRepository;
        _clipboardService = clipboardService;
        _settingsRepository = settingsRepository;

        AppendCharCommand = new RelayCommand(param => AppendChar(param?.ToString() ?? ""));
        OperatorCommand = new RelayCommand(param => SetOperator(param?.ToString() ?? ""));
        UnaryOpCommand = new RelayCommand(param => ExecuteUnary(param?.ToString() ?? ""));
        ShiftLeftCommand = new RelayCommand(() => Shift(1, true));
        ShiftRightCommand = new RelayCommand(() => Shift(1, false));
        CalculateCommand = new RelayCommand(Calculate);
        ClearCommand = new RelayCommand(Clear);
        ClearEntryCommand = new RelayCommand(ClearEntry);
        BackspaceCommand = new RelayCommand(Backspace);
        SwitchBaseCommand = new RelayCommand(param => SwitchBase(param?.ToString() ?? "DEC"));
        SwitchBitSizeCommand = new RelayCommand(param => SwitchBitSize(param?.ToString() ?? "QWORD"));
        CopyCommand = new RelayCommand(CopyResult);

        UpdateAllRepresentations();
    }

    public void AppendChar(string ch)
    {
        StatusMessage = "";
        if (!IsValidCharForBase(ch, ActiveBase)) return;

        if (_isNewEntry || CurrentInput == "0")
        {
            CurrentInput = ch;
            _isNewEntry = false;
        }
        else
        {
            CurrentInput += ch;
        }

        if (_programmerService.TryParse(CurrentInput, ActiveBase, ActiveBitSize, out long val))
        {
            _currentValue = val;
            UpdateAllRepresentations();
        }
    }

    private static bool IsValidCharForBase(string ch, NumberBase nBase)
    {
        if (ch.Length != 1) return false;
        char c = char.ToUpperInvariant(ch[0]);

        return nBase switch
        {
            NumberBase.Binary => c == '0' || c == '1',
            NumberBase.Octal => c >= '0' && c <= '7',
            NumberBase.Decimal => char.IsDigit(c),
            NumberBase.Hexadecimal => char.IsDigit(c) || (c >= 'A' && c <= 'F'),
            _ => false
        };
    }

    public void SetOperator(string op)
    {
        _pendingValue = _currentValue;
        _pendingOp = op;
        _isNewEntry = true;
    }

    public void ExecuteUnary(string op)
    {
        _currentValue = _programmerService.PerformUnaryOp(op, _currentValue, ActiveBitSize);
        UpdateAllRepresentations();
        CurrentInput = _programmerService.FormatNumber(_currentValue, ActiveBase, ActiveBitSize);
        _isNewEntry = true;
    }

    public void Shift(int count, bool isLeft)
    {
        _currentValue = isLeft
            ? _programmerService.ShiftLeft(_currentValue, count, ActiveBitSize)
            : _programmerService.ShiftRight(_currentValue, count, ActiveBitSize);

        UpdateAllRepresentations();
        CurrentInput = _programmerService.FormatNumber(_currentValue, ActiveBase, ActiveBitSize);
        _isNewEntry = true;
    }

    public void Calculate()
    {
        if (_pendingValue.HasValue && !string.IsNullOrEmpty(_pendingOp))
        {
            long res = _programmerService.EvaluateBitwise(_pendingValue.Value, _pendingOp, _currentValue, ActiveBitSize);
            
            var settings = _settingsRepository.LoadSettings();
            if (settings.EnableHistorySaving)
            {
                _historyRepository.AddAsync(new CalculationRecord
                {
                    Expression = $"{_pendingValue.Value} {_pendingOp} {_currentValue}",
                    Result = res.ToString(),
                    Mode = CalculatorMode.Programmer,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            _currentValue = res;
            _pendingValue = null;
            _pendingOp = "";
            UpdateAllRepresentations();
            CurrentInput = _programmerService.FormatNumber(_currentValue, ActiveBase, ActiveBitSize);
            _isNewEntry = true;
        }
    }

    public void Clear()
    {
        _currentValue = 0;
        _pendingValue = null;
        _pendingOp = "";
        _isNewEntry = true;
        UpdateAllRepresentations();
        CurrentInput = "0";
    }

    public void ClearEntry()
    {
        _currentValue = 0;
        _isNewEntry = true;
        UpdateAllRepresentations();
        CurrentInput = "0";
    }

    public void Backspace()
    {
        if (CurrentInput.Length > 1)
        {
            CurrentInput = CurrentInput[..^1];
        }
        else
        {
            CurrentInput = "0";
            _isNewEntry = true;
        }

        if (_programmerService.TryParse(CurrentInput, ActiveBase, ActiveBitSize, out long val))
        {
            _currentValue = val;
            UpdateAllRepresentations();
        }
    }

    private void SwitchBase(string b)
    {
        ActiveBase = b.ToUpperInvariant() switch
        {
            "HEX" => NumberBase.Hexadecimal,
            "DEC" => NumberBase.Decimal,
            "OCT" => NumberBase.Octal,
            "BIN" => NumberBase.Binary,
            _ => NumberBase.Decimal
        };
    }

    private void SwitchBitSize(string s)
    {
        ActiveBitSize = s.ToUpperInvariant() switch
        {
            "QWORD" => BitSize.Qword,
            "DWORD" => BitSize.Dword,
            "WORD" => BitSize.Word,
            "BYTE" => BitSize.Byte,
            _ => BitSize.Qword
        };
    }

    private void UpdateAllRepresentations()
    {
        HexDisplay = _programmerService.FormatNumber(_currentValue, NumberBase.Hexadecimal, ActiveBitSize);
        DecDisplay = _programmerService.FormatNumber(_currentValue, NumberBase.Decimal, ActiveBitSize);
        OctDisplay = _programmerService.FormatNumber(_currentValue, NumberBase.Octal, ActiveBitSize);
        BinDisplay = _programmerService.FormatNumber(_currentValue, NumberBase.Binary, ActiveBitSize);
    }

    public void CopyResult()
    {
        _clipboardService.SetText(CurrentInput);
        StatusMessage = "Copied to clipboard!";
    }
}
