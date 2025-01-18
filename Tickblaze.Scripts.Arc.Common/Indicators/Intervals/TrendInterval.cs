namespace Tickblaze.Scripts.Arc.Common;

public class TrendInterval : Interval, IXPositionable<TrendInterval>
{
	public required StrictTrend Trend { get; init; }
}
