using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Core;

public class Flooding : Indicator
{
	[AllowNull]
	private Series<Trend> _trends;

	[AllowNull]
	private DrawingPartDictionary<int, FloodingInterval> _intervals;

	public bool IsMtf { get; init; }

	public required ISeries<Trend> Sources { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

    protected override void Initialize()
    {
		_intervals = [];
    }

    protected override void Calculate(int barIndex)
    {
		var trends = Sources
			.Distinct()
			.ToArray();

		var trend = Trend.None;
		var previousTrend = barIndex is 0
			? Trend.None : _trends[barIndex - 1];

		if (trends is [var singleTrend])
		{
			trend = singleTrend;
		}

		_intervals.Remove(barIndex);

		if (trend.EnumEquals(previousTrend))
		{

		}
    }
}
