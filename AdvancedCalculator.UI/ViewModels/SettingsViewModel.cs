using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;
using AdvancedCalculator.UI.Services;

namespace AdvancedCalculator.UI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepository;
    private AppSettings _settings;

    public AppTheme SelectedTheme
    {
        get => _settings.Theme;
        set
        {
            if (_settings.Theme != value)
            {
                _settings.Theme = value;
                OnPropertyChanged();
                ThemeManager.ApplyTheme(value);
                Save();
            }
        }
    }

    public string SelectedLanguage
    {
        get => _settings.LanguageCode;
        set
        {
            if (_settings.LanguageCode != value)
            {
                _settings.LanguageCode = value;
                OnPropertyChanged();
                Save();
            }
        }
    }

    public int DecimalPrecision
    {
        get => _settings.DecimalPrecision;
        set
        {
            if (_settings.DecimalPrecision != value)
            {
                _settings.DecimalPrecision = Math.Clamp(value, 0, 15);
                OnPropertyChanged();
                Save();
            }
        }
    }

    public bool AlwaysOnTop
    {
        get => _settings.AlwaysOnTop;
        set
        {
            if (_settings.AlwaysOnTop != value)
            {
                _settings.AlwaysOnTop = value;
                OnPropertyChanged();
                Save();
            }
        }
    }

    public bool AutoCopyResult
    {
        get => _settings.AutoCopyResult;
        set
        {
            if (_settings.AutoCopyResult != value)
            {
                _settings.AutoCopyResult = value;
                OnPropertyChanged();
                Save();
            }
        }
    }

    public bool EnableHistorySaving
    {
        get => _settings.EnableHistorySaving;
        set
        {
            if (_settings.EnableHistorySaving != value)
            {
                _settings.EnableHistorySaving = value;
                OnPropertyChanged();
                Save();
            }
        }
    }

    public ICommand ResetDefaultsCommand { get; }

    public SettingsViewModel(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
        _settings = _settingsRepository.LoadSettings();

        ResetDefaultsCommand = new RelayCommand(ResetDefaults);
    }

    private void ResetDefaults()
    {
        _settings = new AppSettings();
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(DecimalPrecision));
        OnPropertyChanged(nameof(AlwaysOnTop));
        OnPropertyChanged(nameof(AutoCopyResult));
        OnPropertyChanged(nameof(EnableHistorySaving));

        ThemeManager.ApplyTheme(_settings.Theme);
        Save();
    }

    private void Save()
    {
        _settingsRepository.SaveSettings(_settings);
    }
}
