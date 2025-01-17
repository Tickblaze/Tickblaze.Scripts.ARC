using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Tickblaze.Scripts.Arc.Common;

public static class ResourceDictionaries
{
	public static readonly ResourceDictionary DefaultResources = new()
	{
		Source = new Uri("/Tickblaze.Scripts.Arc.Common;component/Resources/Resources.xaml", UriKind.Relative),
	};

	public static TResource LoadResource<TResource>(string resourceName)
	{
		var assembly = Assembly.GetCallingAssembly();

		using var stream = assembly.GetManifestResourceStream(resourceName);

		var resource = XamlReader.Load(stream);

		return (TResource) resource;
	}
}
