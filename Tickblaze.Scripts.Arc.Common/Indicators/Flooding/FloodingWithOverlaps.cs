using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public class FloodingWithOverlaps : Flooding
{
    public override required ISeries<Trend>[] TrendSeriesCollection
	{
		get => base.TrendSeriesCollection;
		init
		{
			if (value is not [var firstTrendSeries, var secondTrendSeries])
			{
				throw new ArgumentException(string.Empty, nameof(TrendSeriesCollection));
			}

			base.TrendSeriesCollection = value;

			FirstTrendSeries = firstTrendSeries;
			SecondTrendSeries = secondTrendSeries;
		}
	}

	[AllowNull]
	public ISeries<Trend> FirstTrendSeries { get; private init; }

	[AllowNull]
	public ISeries<Trend> SecondTrendSeries { get; private init; }

	public required Color OverlapUpTrendColor { get; init; }

	public required Color OverlapDownTrendColor { get; init; }

    protected override bool TryGetCurrentValues(out Trend currentTrend, out Color currentColor)
    {
		var firstTrend = FirstTrendSeries.GetLastOrDefault(Trend.None);
		var secondTrend = SecondTrendSeries.GetLastOrDefault(Trend.None);
		var aggregatedTrend = firstTrend.ToSignum() + secondTrend.ToSignum();

		if (aggregatedTrend is 0 or < -2 or > 2)
        {
			currentTrend = default;
			currentColor = default;

			return false;
        }

        currentTrend = aggregatedTrend.ToTrend();
        currentColor = aggregatedTrend switch
        {
            -2 => OverlapDownTrendColor,
            -1 => DownTrendColor,
            1 => UpTrendColor,
            2 => OverlapUpTrendColor,
            _ => throw new UnreachableException(),
        };

		return true;
    }
}
