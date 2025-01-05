using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public class Flooding : Indicator
{
	[AllowNull]
	private Series<Trend> _trends;

	[AllowNull]
	private DrawingPartDictionary<int, FloodingInterval> _intervals;

	public bool IsMtf { get; init; }

	public required ISeries<Trend> SourceTrends { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

    protected override void Initialize()
    {
		_intervals = [];
    }

    protected override void Calculate(int barIndex)
    {
        Clear(barIndex);

        var trends = SourceTrends
            .Distinct()
            .ToArray();

        var trend = Trend.None;
        var previousTrend = barIndex is 0
            ? Trend.None : _trends[barIndex - 1];

        if (trends is [var singleTrend])
        {
            trend = singleTrend;
        }



        if (trend.EnumEquals(previousTrend))
        {

        }
    }

    private void Clear(int barIndex)
    {
        _intervals.Remove(barIndex);
    }

    public override void OnRender(IDrawingContext drawingContext)
    {

    }
}
