using System.ComponentModel;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Flooding : ChildIndicator
{
	public virtual required ISeries<Trend>[] TrendSeriesCollection { get; init; }

	public required bool IsMtf { get; init; }

	public required Color UpTrendColor { get; init; }

	public required Color DownTrendColor { get; init; }

    protected override void Initialize()
	{
		if (IsInitialized)
		{
			return;
		}

		IsInitialized = true;
	}

	protected Trend GetTrend(ISeries<Trend> series, int barIndex)
    {
		return IsMtf
			? series.GetLastOrDefault(Trend.None)
			: series.GetAtOrDefault(barIndex, Trend.None);
	}

	protected virtual Color? TryGetCurrentValues(int barIndex)
	{
		var currentTrends = TrendSeriesCollection
			.Select(series => GetTrend(series, barIndex))
			.Distinct()
			.ToArray();

		if (currentTrends is not [var firstTrend] || firstTrend is Trend.None)
		{
			return default;
		}

		return firstTrend.ToStrictTrend()
			.Map(UpTrendColor, DownTrendColor);
	}

	protected override void Calculate(int barIndex)
    {
		BackgroundColor[barIndex] = TryGetCurrentValues(barIndex);
	}
}
