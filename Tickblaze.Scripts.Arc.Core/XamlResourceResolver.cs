using MahApps.Metro.Behaviors;
using System.Runtime.CompilerServices;

namespace Tickblaze.Scripts.Arc.Core;

public static class XamlResourceResolver
{
	[ModuleInitializer]
	public static void LoadWpfResourceReferences()
	{
		// Todo; document this.
		var _ = typeof(TiltBehavior);
	}
}
