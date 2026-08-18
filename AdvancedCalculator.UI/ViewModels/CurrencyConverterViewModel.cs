using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;
using AdvancedCalculator.UI.Helpers;

namespace AdvancedCalculator.UI.ViewModels;

public class CurrencyConverterViewModel : ViewModelBase
{
    private readonly ICurrencyService _currencyService;
    private readonly IClipboardService _clipboardService;

    private CurrencyRate? _fromCurrency;
    private CurrencyRate? _toCurrency;
    private string _fromAmount = "1";
    private string _toAmount = "0";
    private string _rateInfo = "";
    private string _lastUpdated = "";

    public ObservableCollection<CurrencyRate> Currencies { get; } = new();

    public CurrencyRate? FromCurrency
    {
        get => _fromCurrency;
        set
        {
            if (SetProperty(ref _fromCurrency, value))
            {
                Recalculate();
            }
        }
    }

    public CurrencyRate? ToCurrency
    {
        get => _toCurrency;
        set
        {
            if (SetProperty(ref _toCurrency, value))
            {
                Recalculate();
            }
        }
    }

    public string FromAmount
    {
        get => _fromAmount;
        set
        {
            if (SetProperty(ref _fromAmount, value))
            {
                Recalculate();
            }
        }
    }

    public string ToAmount
    {
        get => _toAmount;
        set => SetProperty(ref _toAmount, value);
    }

    public string RateInfo
    {
        get => _rateInfo;
        set => SetProperty(ref _rateInfo, value);
    }

    public string LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    public ICommand SwapCurrenciesCommand { get; }
    public ICommand AppendDigitCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand BackspaceCommand { get; }
    public ICommand CopyCommand { get; }

    public CurrencyConverterViewModel(ICurrencyService currencyService, IClipboardService clipboardService)
    {
        _currencyService = currencyService;
        _clipboardService = clipboardService;

        SwapCurrenciesCommand = new RelayCommand(SwapCurrencies);
        AppendDigitCommand = new RelayCommand(param => AppendDigit(param?.ToString() ?? ""));
        ClearCommand = new RelayCommand(() => FromAmount = "0");
        BackspaceCommand = new RelayCommand(Backspace);
        CopyCommand = new RelayCommand(() => _clipboardService.SetText(ToAmount));

        _ = LoadCurrenciesAsync();
    }

    private async Task LoadCurrenciesAsync()
    {
        var rates = await _currencyService.GetRatesAsync();
        Currencies.Clear();
        foreach (var r in rates)
        {
            Currencies.Add(r);
        }

        FromCurrency = Currencies.FirstOrDefault(c => c.CurrencyCode == "USD");
        ToCurrency = Currencies.FirstOrDefault(c => c.CurrencyCode == "EUR") ?? Currencies.FirstOrDefault();

        var updated = await _currencyService.GetLastUpdatedTimeAsync();
        LastUpdated = $"Exchange rates updated: {updated:yyyy-MM-dd HH:mm} UTC";

        Recalculate();
    }

    public void AppendDigit(string digit)
    {
        if (FromAmount == "0" && digit != ".")
        {
            FromAmount = digit;
        }
        else if (digit == "." && FromAmount.Contains('.'))
        {
            // Ignore duplicate dot
        }
        else
        {
            FromAmount += digit;
        }
    }

    public void Backspace()
    {
        if (FromAmount.Length > 1)
        {
            FromAmount = FromAmount[..^1];
        }
        else
        {
            FromAmount = "0";
        }
    }

    private void SwapCurrencies()
    {
        var temp = FromCurrency;
        FromCurrency = ToCurrency;
        ToCurrency = temp;
    }

    private void Recalculate()
    {
        if (FromCurrency == null || ToCurrency == null) return;

        if (decimal.TryParse(FromAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amt))
        {
            decimal res = _currencyService.Convert(amt, FromCurrency.CurrencyCode, ToCurrency.CurrencyCode, Currencies.ToList());
            ToAmount = res.ToString("F4", CultureInfo.InvariantCulture);

            decimal unitRate = _currencyService.Convert(1, FromCurrency.CurrencyCode, ToCurrency.CurrencyCode, Currencies.ToList());
            RateInfo = $"1 {FromCurrency.CurrencyCode} = {unitRate:F4} {ToCurrency.CurrencyCode}";
        }
        else
        {
            ToAmount = "0";
            RateInfo = "";
        }
    }
}
