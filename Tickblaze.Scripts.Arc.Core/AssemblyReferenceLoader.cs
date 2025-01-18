using MahApps.Metro.Behaviors;
using System.Runtime.CompilerServices;

namespace Tickblaze.Scripts.Arc.Core;

public static class AssemblyReferenceLoader
{
	[ModuleInitializer]
	public static void LoadReferences()
	{
		// Todo; document this.
		var _ = typeof(TiltBehavior);
	}
}
