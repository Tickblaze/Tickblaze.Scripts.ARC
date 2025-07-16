using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tickblaze.Community;

public class ButtonMenuItem : MenuItem
{
    public static readonly DependencyProperty ActionCommandProperty = DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(ButtonMenuItem));
    public static readonly DependencyProperty ActionCommandParameterProperty = DependencyProperty.Register(nameof(ActionCommandParameter), typeof(object), typeof(ButtonMenuItem));
    
    public ICommand ActionCommand
    {
        get => (ICommand) GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }
}