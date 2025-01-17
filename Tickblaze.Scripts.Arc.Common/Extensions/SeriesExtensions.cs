namespace Tickblaze.Scripts.Arc.Common;

public static class SeriesExtensions
{
	public static int GetBarIndex(this BarSeries barSeries, DateTime timeUtc)
	{
		var barIndex = barSeries.Slice(timeUtc).FirstOrDefault(-1);

		if (barIndex is -1)
		{
			return -1;
		}

		if (barSeries[barIndex] is var bar
			&& bar is not null
			&& DateTime.Equals(timeUtc, bar.Time))
		{
			return barIndex;
		}

		return barIndex - 1;
	}

    public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
    {
        return new SeriesTransform<TSource, TDestination>(sourceSeries, selector);
    }

	public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<int, TSource, TDestination> selector)
	{
		return new SeriesTransform<TSource, TDestination>(sourceSeries, selector);
	}

	public static ISeries<TDestination> MapAndCache<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
	{
		return new SeriesTransformCached<TSource, TDestination>(sourceSeries, selector);
	}

	public static ISeries<TDestination> MapAndCache<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<int, TSource, TDestination> selector)
	{
		return new SeriesTransformCached<TSource, TDestination>(sourceSeries, selector);
	}
}
