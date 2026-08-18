using System.Windows;
using AdvancedCalculator.Application.ExpressionEngine;
using AdvancedCalculator.Application.Services;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Infrastructure.Repositories;
using AdvancedCalculator.UI.Services;
using AdvancedCalculator.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedCalculator.UI;

public partial class App : System.Windows.Application
{
    private ServiceProvider _serviceProvider = null!;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core & Application
        services.AddSingleton<IExpressionEngine, ExpressionEngine>();
        services.AddSingleton<IProgrammerService, ProgrammerService>();
        services.AddSingleton<IUnitConverterService, UnitConverterService>();
        services.AddSingleton<ICurrencyService, CurrencyService>();

        // Infrastructure
        services.AddSingleton<IHistoryRepository, SqliteHistoryRepository>();
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();
        services.AddSingleton<IClipboardService, WpfClipboardService>();

        // ViewModels (Singletons to preserve state when switching modes)
        services.AddSingleton<StandardCalculatorViewModel>();
        services.AddSingleton<ScientificCalculatorViewModel>();
        services.AddSingleton<ProgrammerCalculatorViewModel>();
        services.AddSingleton<UnitConverterViewModel>();
        services.AddSingleton<CurrencyConverterViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();

        // Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsRepo = _serviceProvider.GetRequiredService<ISettingsRepository>();
        var settings = settingsRepo.LoadSettings();
        ThemeManager.ApplyTheme(settings.Theme);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
