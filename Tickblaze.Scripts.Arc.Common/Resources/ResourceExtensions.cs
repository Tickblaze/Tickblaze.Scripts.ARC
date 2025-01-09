using System.Windows;

namespace Tickblaze.Scripts.Arc.Common;

public static class ResourceExtensions
{
	public static TResource FindResource<TResource>(Uri uri)
	{
		var component = Application.LoadComponent(uri);

		if (component is not TResource resource)
		{
			throw new InvalidCastException(nameof(FindResource));
		}

		return resource;
	}
}
