using System.Runtime.CompilerServices;

namespace Tickblaze.Community;

public static class IndicatorExtensions
{
	public static void Reinitialize(this Indicator indicator, Indicator parentIndicator)
	{
		ArgumentNullException.ThrowIfNull(indicator);

		Initialize(indicator, parentIndicator.Chart, parentIndicator.Bars);

		indicator.Calculate();
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Initialize")]
	private static extern void Initialize(SymbolScript symbolScript, IChart chart, BarSeries? bars, bool isReinitializationRequired = false);
}
