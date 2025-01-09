using System.Windows;

namespace Tickblaze.Scripts.Arc.Common;

public static class ResourceDictionaries
{
	// Todo: documentation.
	public static readonly ResourceDictionary BaseResources = new()
	{
		Source = new Uri("/Tickblaze.Scripts.Arc.Common;component/Resources/BaseResources.xaml", UriKind.Relative),
	};

	public static readonly ResourceDictionary DefaultResources = new()
	{
		Source = new Uri("/Tickblaze.Scripts.Arc.Common;component/Resources/DefaultResources.xaml", UriKind.Relative),
	};
}
