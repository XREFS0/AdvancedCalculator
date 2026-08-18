using System.Windows;
using AdvancedCalculator.Core.Interfaces;

namespace AdvancedCalculator.UI.Services;

public class WpfClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch
        {
            // Ignored if clipboard locked
        }
    }

    public string GetText()
    {
        try
        {
            return Clipboard.GetText();
        }
        catch
        {
            return string.Empty;
        }
    }
}
