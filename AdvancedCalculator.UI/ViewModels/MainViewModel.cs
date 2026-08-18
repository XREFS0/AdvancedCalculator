using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;
    private CalculatorMode _currentMode = CalculatorMode.Standard;
    private bool _isAlwaysOnTop = false;

    public StandardCalculatorViewModel StandardVM { get; }
    public ScientificCalculatorViewModel ScientificVM { get; }
    public ProgrammerCalculatorViewModel ProgrammerVM { get; }
    public UnitConverterViewModel UnitConverterVM { get; }
    public CurrencyConverterViewModel CurrencyVM { get; }
    public HistoryViewModel HistoryVM { get; }
    public SettingsViewModel SettingsVM { get; }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public CalculatorMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (SetProperty(ref _currentMode, value))
            {
                SwitchToMode(value);
            }
        }
    }

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set => SetProperty(ref _isAlwaysOnTop, value);
    }

    public ICommand NavigateCommand { get; }
    public ICommand ToggleAlwaysOnTopCommand { get; }

    public MainViewModel(
        StandardCalculatorViewModel standardVM,
        ScientificCalculatorViewModel scientificVM,
        ProgrammerCalculatorViewModel programmerVM,
        UnitConverterViewModel unitConverterVM,
        CurrencyConverterViewModel currencyVM,
        HistoryViewModel historyVM,
        SettingsViewModel settingsVM)
    {
        StandardVM = standardVM;
        ScientificVM = scientificVM;
        ProgrammerVM = programmerVM;
        UnitConverterVM = unitConverterVM;
        CurrencyVM = currencyVM;
        HistoryVM = historyVM;
        SettingsVM = settingsVM;

        _currentViewModel = standardVM;
        IsAlwaysOnTop = settingsVM.AlwaysOnTop;

        NavigateCommand = new RelayCommand(param =>
        {
            if (param is string modeStr && Enum.TryParse<CalculatorMode>(modeStr, out var mode))
            {
                CurrentMode = mode;
            }
        });

        ToggleAlwaysOnTopCommand = new RelayCommand(() =>
        {
            IsAlwaysOnTop = !IsAlwaysOnTop;
            SettingsVM.AlwaysOnTop = IsAlwaysOnTop;
        });

        HistoryVM.RequestLoadCalculation += record =>
        {
            StandardVM.LoadFromHistory(record);
            CurrentMode = CalculatorMode.Standard;
        };
    }

    private void SwitchToMode(CalculatorMode mode)
    {
        CurrentViewModel = mode switch
        {
            CalculatorMode.Standard => StandardVM,
            CalculatorMode.Scientific => ScientificVM,
            CalculatorMode.Programmer => ProgrammerVM,
            CalculatorMode.UnitConverter => UnitConverterVM,
            CalculatorMode.CurrencyConverter => CurrencyVM,
            CalculatorMode.History => HistoryVM,
            CalculatorMode.Settings => SettingsVM,
            _ => StandardVM
        };
    }
}
