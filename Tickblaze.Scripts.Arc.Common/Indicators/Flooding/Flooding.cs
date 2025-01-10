using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public class Flooding : ChildIndicator
{
	[AllowNull]
	private Series<Trend> _trends;

	[AllowNull]
	private DrawingPartDictionary<int, FloodingInterval> _intervals;

	public required ISeries<Trend>[] SourceTrends { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

	private FloodingInterval LastInterval => _intervals.LastDrawingPart;

    protected override void Initialize()
	{
		_trends = [];

		_intervals = [];
	}

	protected override void Calculate(int barIndex)
    {
        Reset(barIndex);

        var currentTrends = SourceTrends
			.Select(series => series.GetLastOrDefault(Trend.None))
			.Distinct()
			.ToArray();

        var previousTrend = _trends.GetAtOrDefault(barIndex - 1, Trend.None);

        if (currentTrends is not [var currentTrend] || currentTrend is Trend.None)
        {
			return;
        }

        if (previousTrend is Trend.None
			|| !currentTrend.EnumEquals(previousTrend))
        {
			var currentStrictTrend = currentTrend.ToStrictTrend();
			var color = currentStrictTrend.Map(UpTrendColor, DownTrendColor);

			var currentInterval = new FloodingInterval
			{
				Color = color,
				EndBarIndex = barIndex,
				StartBarIndex = barIndex,
				Trend = currentStrictTrend,
			};

			_intervals.AddOrUpdate(currentInterval);
        }
		else if (currentTrend.EnumEquals(previousTrend))
		{
			LastInterval.EndBarIndex = barIndex;
		}
    }

    private void Reset(int barIndex)
    {
        _intervals.Remove(barIndex);

		if (!_intervals.IsEmpty && barIndex.Equals(LastInterval.EndBarIndex))
		{
			LastInterval.EndBarIndex--;
		}
	}

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (Chart is null
			|| ChartScale is null
			|| RenderTarget is null)
		{
			throw new InvalidOperationException(nameof(OnRender));
		}

		var visibleBoundary = RenderTarget.GetVisibleBoundary();
		var visibleIntervals = _intervals.GetVisibleDrawingParts(visibleBoundary);

		foreach (var visibleInterval in visibleIntervals)
		{
			var startY = ChartScale.GetBottomY();
			var startX = Chart.GetXCoordinateByBarIndex(visibleInterval.StartBarIndex);

			var endY = ChartScale.GetTopY();
			var endX = Chart.GetXCoordinateByBarIndex(visibleInterval.EndBarIndex);

			drawingContext.DrawRectangle(startX, startY, endX, endY, visibleInterval.Color);
		}
    }
}
