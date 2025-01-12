using MahApps.Metro.Behaviors;
using System.Runtime.CompilerServices;

namespace Tickblaze.Scripts.Arc.Core;

public static class XamlResourceResolver
{
	[ModuleInitializer]
	public static void ReferenceXamlResources()
	{
		var _ = new TiltBehavior();
	}
}
