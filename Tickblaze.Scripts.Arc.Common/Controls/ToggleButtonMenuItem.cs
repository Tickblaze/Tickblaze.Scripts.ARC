using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Scripts.Arc.Common;

public class ToggleButtonMenuItem : MenuItem
{
    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(ToggleButtonMenuItem));
    
    public bool IsOn
    {
        get => (bool) GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }
}
