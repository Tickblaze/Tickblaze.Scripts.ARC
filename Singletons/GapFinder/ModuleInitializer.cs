using MahApps.Metro.Behaviors;
using System.Runtime.CompilerServices;

namespace Tickblaze.Community;

public static class ModuleInitializer
{
	[ModuleInitializer]
	public static void LoadReferences()
	{
		// Todo: document this.
		var _ = typeof(TiltBehavior);
	}
}
