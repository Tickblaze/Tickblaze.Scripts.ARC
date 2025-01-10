using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Scripts.Arc.Common;

public class NumericMenuItem : MenuItem
{
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericMenuItem));

	public double Value
    {
        get => (double) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
