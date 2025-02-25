namespace Tickblaze.Community;

public class TrendInterval : Interval, IXPositionable<TrendInterval>
{
	public required StrictTrend Trend { get; init; }
}
