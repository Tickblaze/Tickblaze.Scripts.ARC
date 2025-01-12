using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Flooding : CommonIndicator
{
	[AllowNull]
	private Series<Trend> _trends;

	[AllowNull]
	private DrawingPartDictionary<int, FloodingInterval> _intervals;

	public virtual required ISeries<Trend>[] TrendSeriesCollection { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

	protected FloodingInterval LastInterval => _intervals.LastDrawingPart;

    protected override void Initialize()
	{
		_trends = [];

		_intervals = [];
	}

	protected virtual bool TryGetCurrentValues(out Trend currentTrend, out Color currentColor)
	{
		var currentTrends = TrendSeriesCollection
			.Select(series => series.GetLastOrDefault(Trend.None))
			.Distinct()
			.ToArray();

		if (currentTrends is not [var firstTrend] || firstTrend is Trend.None)
		{
			currentTrend = default;
			currentColor = default;

			return false;
		}

		currentTrend = firstTrend;

		currentColor = firstTrend.ToStrictTrend()
			.Map(UpTrendColor, DownTrendColor);

		return true;
	}

	protected override void Calculate(int barIndex)
    {
		Reset(barIndex);

		var previousTrend = _trends.GetAtOrDefault(barIndex - 1, Trend.None);

		if (!TryGetCurrentValues(out var currentTrend, out var currentColor))
		{
			return;
		}

		_trends[barIndex] = currentTrend;

		if (previousTrend is Trend.None
			|| !currentTrend.EnumEquals(previousTrend)
			|| currentColor.Equals(LastInterval.Color))
		{
			var strictTrend = currentTrend.ToStrictTrend();

			var currentInterval = new FloodingInterval
			{
				Trend = strictTrend,
				Color = currentColor,
				EndBarIndex = barIndex,
				StartBarIndex = barIndex,
			};

			_intervals.AddOrUpdate(currentInterval);
		}
		else if (currentTrend.EnumEquals(previousTrend))
		{
			LastInterval.EndBarIndex = barIndex;
		}
	}

    protected void Reset(int barIndex)
    {
        _intervals.Remove(barIndex);

		_trends[barIndex] = Trend.None;

		if (!_intervals.IsEmpty && barIndex.Equals(LastInterval.EndBarIndex))
		{
			LastInterval.EndBarIndex--;
		}
	}

	public override void OnRender(IDrawingContext drawingContext)
    {
		if (Chart is null || ChartScale is null || RenderTarget is null)
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
