using System.Text.Json;
using AdvancedCalculator.Core.Interfaces;
using AdvancedCalculator.Core.Models;

namespace AdvancedCalculator.Infrastructure.Repositories;

public class JsonSettingsRepository : ISettingsRepository
{
    private readonly string _settingsPath;

    public JsonSettingsRepository(string? customPath = null)
    {
        if (string.IsNullOrWhiteSpace(customPath))
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "AdvancedCalculator");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
        }
        else
        {
            _settingsPath = customPath;
        }
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch
        {
            // Fallback to default
        }

        var defaultSettings = new AppSettings();
        SaveSettings(defaultSettings);
        return defaultSettings;
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Ignored or logged
        }
    }
}
