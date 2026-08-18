using System.Windows;
using AdvancedCalculator.Core.Enums;

namespace AdvancedCalculator.UI.Services;

public class ThemeManager
{
    public static void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        string themeUri = theme switch
        {
            AppTheme.Light => "Themes/LightTheme.xaml",
            AppTheme.Dark => "Themes/DarkTheme.xaml",
            _ => "Themes/DarkTheme.xaml"
        };

        var newDict = new ResourceDictionary
        {
            Source = new Uri(themeUri, UriKind.Relative)
        };

        // Merge or replace theme resource dictionary
        var merged = app.Resources.MergedDictionaries;
        var existingTheme = merged.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("DarkTheme") || d.Source.OriginalString.Contains("LightTheme")));

        if (existingTheme != null)
        {
            merged.Remove(existingTheme);
        }

        merged.Insert(0, newDict);
    }
}
