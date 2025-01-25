using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Api.Models;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Flooding : ChildIndicator
{
	[AllowNull]
	private Series<Trend> _trends;

	[AllowNull]
	private DrawingPartDictionary<int, FloodingInterval> _intervals;

	public virtual required ISeries<Trend>[] TrendSeriesCollection { get; init; }

	public required bool IsMtf { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

	protected FloodingInterval LastInterval => _intervals.LastDrawingPart;

    protected override void Initialize()
	{
		if (IsInitialized)
		{
			return;
		}

		_trends = [];

		_intervals = [];

		IsInitialized = true;
	}

	protected Trend GetTrend(ISeries<Trend> series, int barIndex)
    {
		return IsMtf
			? series.GetLastOrDefault(Trend.None)
			: series.GetAtOrDefault(barIndex, Trend.None);
	}

	protected virtual bool TryGetCurrentValues(int barIndex, out Trend currentTrend, out Color currentColor)
	{
		var currentTrends = TrendSeriesCollection
			.Select(series => GetTrend(series, barIndex))
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

		if (!TryGetCurrentValues(barIndex, out var currentTrend, out var currentColor))
		{
			return;
		}

		_trends[barIndex] = currentTrend;

		if (previousTrend is Trend.None
			|| !currentTrend.EnumEquals(previousTrend)
			|| !currentColor.Equals(LastInterval.Color))
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

		var absoluteBarWidth = Chart.GetAbsoluteBarWidth();
		var visibleBoundary = RenderTarget.GetVisibleBoundary();
		var visibleIntervals = _intervals.GetVisibleDrawingParts(visibleBoundary);

		foreach (var visibleInterval in visibleIntervals)
		{
			var startY = ChartScale.GetBottomY();
			var startX = Chart.GetXCoordinateByBarIndex(visibleInterval.StartBarIndex) - absoluteBarWidth / 2.0;

			var endY = ChartScale.GetTopY();
			var endX = Chart.GetXCoordinateByBarIndex(visibleInterval.EndBarIndex) + absoluteBarWidth / 2.0;

			drawingContext.DrawRectangle(startX, startY, endX, endY, visibleInterval.Color);
		}
    }
}
