using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Community;

public class NumericMenuItem : MenuItem
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericMenuItem));
	public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericMenuItem), new(double.MinValue));
	public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericMenuItem), new(double.MaxValue));
	public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(nameof(Interval), typeof(double), typeof(NumericMenuItem), new(1.0d));
	public static readonly DependencyProperty NumericInputModeProperty = DependencyProperty.Register(nameof(NumericInputMode), typeof(NumericInput), typeof(NumericMenuItem), new(NumericInput.All));
	
	public double Value
	{
		get => (double) GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public double Interval
    {
        get => (double) GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }
	
	public double Minimum
	{
		get => (double) GetValue(MinimumProperty);
		set => SetValue(MinimumProperty, value);
	}

	public double Maximum
	{
		get => (double) GetValue(MaximumProperty);
		set => SetValue(MaximumProperty, value);
	}

	public NumericInput NumericInputMode
	{
		get => (NumericInput) GetValue(NumericInputModeProperty);
		set => SetValue(NumericInputModeProperty, value);
	}
}