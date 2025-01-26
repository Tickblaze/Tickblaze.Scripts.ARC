namespace Tickblaze.Scripts.Arc.Common;

public static class SeriesExtensions
{
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
		return new CachedSeriesTransform<TSource, TDestination>(sourceSeries, selector);
	}

	public static ISeries<TDestination> MapAndCache<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<int, TSource, TDestination> selector)
	{
		return new CachedSeriesTransform<TSource, TDestination>(sourceSeries, selector);
	}
}
