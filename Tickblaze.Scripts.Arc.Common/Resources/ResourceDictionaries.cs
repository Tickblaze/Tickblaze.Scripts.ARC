using System.Windows;

namespace Tickblaze.Scripts.Arc.Common;

public static class ResourceDictionaries
{
	public static readonly ResourceDictionary DefaultResources = new()
	{
		Source = new Uri("/Tickblaze.Scripts.Arc.Common;component/Resources/Resources.xaml", UriKind.Relative),
	};
}
