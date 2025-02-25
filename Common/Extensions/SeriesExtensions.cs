namespace Tickblaze.Community;

public static class SeriesExtensions
{
	public static int GetBarIndex(this BarSeries bars, DateTime timeUtc)
	{
		ArgumentNullException.ThrowIfNull(bars);

		if (bars.Count is 0)
		{
			return -1;
		}

		if (bars is [var firstBar, ..] && DateTime.Equals(firstBar.Time, timeUtc))
		{
			return 0;
		}

		var lastBarIndex = bars.Count - 1;

		var barIndex = bars
			.Slice(timeUtc)
			.FirstOrDefault(lastBarIndex);

		var isFutureBar = !barIndex.Equals(lastBarIndex)
			&& !DateTime.Equals(timeUtc, bars.Time[barIndex]);

		return barIndex - Convert.ToInt32(isFutureBar);
	}
}
