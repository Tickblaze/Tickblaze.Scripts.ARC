using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

namespace Tickblaze.Community;

public static class ResourceDictionaries
{
	public static readonly ResourceDictionary DefaultResources = new()
	{
		Source = new Uri("/Common;v2.0.0.0;component/Resources/Resources.xaml", UriKind.Relative),
	};

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static TResource LoadResource<TResource>(string resourceName)
	{
		var assembly = Assembly.GetCallingAssembly();
		var resolvedResourceName = ResolveResourceName(assembly, resourceName);

		using var stream = assembly.GetManifestResourceStream(resolvedResourceName);

		if (stream is null)
		{
			throw new InvalidOperationException(
				$"Resource '{resolvedResourceName}' was found in assembly '{assembly.GetName().Name}', but could not be loaded.");
		}

		var resource = XamlReader.Load(stream);

		return (TResource) resource;
	}

	private static string ResolveResourceName(Assembly assembly, string resourceName)
	{
		var resourceNames = assembly.GetManifestResourceNames();

		if (resourceNames.Contains(resourceName, StringComparer.Ordinal))
		{
			return resourceName;
		}

		var matchingResourceNames = resourceNames
			.Where(resourceNameCandidate => resourceNameCandidate.EndsWith(resourceName, StringComparison.Ordinal))
			.ToArray();

		return matchingResourceNames.Length switch
		{
			1 => matchingResourceNames[0],
			0 => throw new InvalidOperationException(
				$"Resource '{resourceName}' was not found in assembly '{assembly.GetName().Name}'. "
				+ $"Available resources: {FormatResourceNames(resourceNames)}."),
			_ => throw new InvalidOperationException(
				$"Resource '{resourceName}' is ambiguous in assembly '{assembly.GetName().Name}'. "
				+ $"Matching resources: {FormatResourceNames(matchingResourceNames)}.")
		};
	}

	private static string FormatResourceNames(IEnumerable<string> resourceNames)
	{
		var resourceNameArray = resourceNames.ToArray();

		return resourceNameArray.Length is 0
			? "<none>"
			: string.Join(", ", resourceNameArray);
	}
}
