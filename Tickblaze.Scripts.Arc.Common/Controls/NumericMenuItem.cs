using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Scripts.Arc.Common;

public class NumericMenuItem : MenuItem
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericMenuItem));
    public static readonly DependencyProperty NumericInputModeProperty = DependencyProperty.Register(nameof(NumericInputMode), typeof(NumericInput), typeof(NumericMenuItem), new(NumericInput.All));

	public double Value
    {
        get => (double) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

	public NumericInput NumericInputMode
    {
        get => (NumericInput) GetValue(NumericInputModeProperty);
        set => SetValue(NumericInputModeProperty, value);
    }
}
