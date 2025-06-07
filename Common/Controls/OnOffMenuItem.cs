using System.Windows;
using System.Windows.Controls;

namespace Tickblaze.Community;

public class OnOffMenuItem : MenuItem
{
    static OnOffMenuItem()
    {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(OnOffMenuItem), new FrameworkPropertyMetadata(typeof(OnOffMenuItem)));
    }

    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(nameof(IsOn), typeof(bool), typeof(OnOffMenuItem));
    public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(nameof(IsThreeState), typeof(bool), typeof(OnOffMenuItem), new(false));
    
    public bool IsOn
    {
        get => (bool) GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

	public bool IsThreeState
    {
        get => (bool) GetValue(IsThreeStateProperty);
        set => SetValue(IsThreeStateProperty, value);
    }
}
