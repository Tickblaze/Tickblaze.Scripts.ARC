using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Test.Wpf;

public class CheckBoxMenuItem : MenuItem
{
    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(CheckBoxMenuItem));
    public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(CheckBoxMenuItem));

	public bool IsOn
    {
        get => (bool) GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

	public string HeaderText
    {
        get => (string) GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }
}
