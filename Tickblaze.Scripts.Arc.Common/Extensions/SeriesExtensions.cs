namespace Tickblaze.Scripts.Arc.Common;

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

		var barIndex = bars.Slice(timeUtc)
			.Append(bars.Count - 1)
			.First();

		var isFutureBar = !DateTime.Equals(timeUtc, bars.Time[barIndex]);

		return barIndex - Convert.ToInt32(isFutureBar);
	}
}
