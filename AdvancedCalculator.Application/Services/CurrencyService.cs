using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Application.Services;

public class CurrencyService : ICurrencyService
{
    private static readonly List<CurrencyRate> DefaultRates = new()
    {
        new() { CurrencyCode = "USD", CurrencyName = "US Dollar", Symbol = "$", RateToBaseUSD = 1.0m },
        new() { CurrencyCode = "EUR", CurrencyName = "Euro", Symbol = "€", RateToBaseUSD = 0.92m },
        new() { CurrencyCode = "GBP", CurrencyName = "British Pound", Symbol = "£", RateToBaseUSD = 0.78m },
        new() { CurrencyCode = "SAR", CurrencyName = "Saudi Riyal", Symbol = "﷼", RateToBaseUSD = 3.75m },
        new() { CurrencyCode = "AED", CurrencyName = "UAE Dirham", Symbol = "د.إ", RateToBaseUSD = 3.67m },
        new() { CurrencyCode = "EGP", CurrencyName = "Egyptian Pound", Symbol = "E£", RateToBaseUSD = 48.50m },
        new() { CurrencyCode = "KWD", CurrencyName = "Kuwaiti Dinar", Symbol = "KD", RateToBaseUSD = 0.31m },
        new() { CurrencyCode = "QAR", CurrencyName = "Qatari Riyal", Symbol = "QR", RateToBaseUSD = 3.64m },
        new() { CurrencyCode = "JPY", CurrencyName = "Japanese Yen", Symbol = "¥", RateToBaseUSD = 155.0m },
        new() { CurrencyCode = "CNY", CurrencyName = "Chinese Yuan", Symbol = "¥", RateToBaseUSD = 7.23m },
        new() { CurrencyCode = "CAD", CurrencyName = "Canadian Dollar", Symbol = "CA$", RateToBaseUSD = 1.36m },
        new() { CurrencyCode = "AUD", CurrencyName = "Australian Dollar", Symbol = "AU$", RateToBaseUSD = 1.51m },
        new() { CurrencyCode = "CHF", CurrencyName = "Swiss Franc", Symbol = "CHF", RateToBaseUSD = 0.90m },
        new() { CurrencyCode = "TRY", CurrencyName = "Turkish Lira", Symbol = "₺", RateToBaseUSD = 32.50m },
        new() { CurrencyCode = "INR", CurrencyName = "Indian Rupee", Symbol = "₹", RateToBaseUSD = 83.45m }
    };

    public Task<IReadOnlyList<CurrencyRate>> GetRatesAsync()
    {
        return Task.FromResult<IReadOnlyList<CurrencyRate>>(DefaultRates);
    }

    public decimal Convert(decimal amount, string fromCode, string toCode, IReadOnlyList<CurrencyRate> rates)
    {
        if (fromCode == toCode) return amount;

        var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == fromCode);
        var toRate = rates.FirstOrDefault(r => r.CurrencyCode == toCode);

        if (fromRate == null || toRate == null)
            throw new ArgumentException("Currency rate not found for conversion.");

        // amount in USD = amount / fromRate.RateToBaseUSD
        // result = amountInUSD * toRate.RateToBaseUSD
        decimal amountInUSD = amount / fromRate.RateToBaseUSD;
        return amountInUSD * toRate.RateToBaseUSD;
    }

    public Task<DateTime> GetLastUpdatedTimeAsync()
    {
        return Task.FromResult(DateTime.UtcNow);
    }
}
