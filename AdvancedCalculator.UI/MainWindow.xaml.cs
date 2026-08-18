using System.Windows;
using System.Windows.Input;
using AdvancedCalculator.Core.Enums;
using AdvancedCalculator.UI.ViewModels;

namespace AdvancedCalculator.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel mainVm) return;

        // Function keys navigation
        if (e.Key == Key.F1) { mainVm.CurrentMode = CalculatorMode.Standard; return; }
        if (e.Key == Key.F2) { mainVm.CurrentMode = CalculatorMode.Scientific; return; }
        if (e.Key == Key.F3) { mainVm.CurrentMode = CalculatorMode.Programmer; return; }
        if (e.Key == Key.F4) { mainVm.CurrentMode = CalculatorMode.UnitConverter; return; }
        if (e.Key == Key.F5) { mainVm.CurrentMode = CalculatorMode.CurrencyConverter; return; }
        if (e.Key == Key.F6) { mainVm.CurrentMode = CalculatorMode.History; return; }

        // Standard Calculator Keybindings
        if (mainVm.CurrentViewModel is StandardCalculatorViewModel stdVm)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                stdVm.AppendDigit(((int)e.Key - (int)Key.D0).ToString());
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                stdVm.AppendDigit(((int)e.Key - (int)Key.NumPad0).ToString());
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                stdVm.AppendDecimal();
                e.Handled = true;
            }
            else if (e.Key == Key.Add || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) != 0))
            {
                stdVm.AppendOperator("+");
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                stdVm.AppendOperator("-");
                e.Handled = true;
            }
            else if (e.Key == Key.Multiply)
            {
                stdVm.AppendOperator("*");
                e.Handled = true;
            }
            else if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
            {
                stdVm.AppendOperator("/");
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) == 0))
            {
                stdVm.Calculate();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                stdVm.Backspace();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                stdVm.Clear();
                e.Handled = true;
            }
        }
        // Scientific Calculator Keybindings
        else if (mainVm.CurrentViewModel is ScientificCalculatorViewModel sciVm)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                sciVm.AppendDigit(((int)e.Key - (int)Key.D0).ToString());
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                sciVm.AppendDigit(((int)e.Key - (int)Key.NumPad0).ToString());
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                sciVm.AppendDecimal();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                sciVm.Calculate();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                sciVm.Backspace();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                sciVm.Clear();
                e.Handled = true;
            }
        }
    }
}