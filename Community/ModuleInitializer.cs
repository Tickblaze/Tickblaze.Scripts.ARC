using MahApps.Metro.Behaviors;
using System.Runtime.CompilerServices;

namespace Tickblaze.Community;

public static class ModuleInitializer
{
	[ModuleInitializer]
	public static void LoadReferences()
	{
		var type = typeof(TiltBehavior);

		Console.Write(type);
	}
}
