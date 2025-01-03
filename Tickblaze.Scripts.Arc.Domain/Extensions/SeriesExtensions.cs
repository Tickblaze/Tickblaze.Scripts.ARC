using Tickblaze.Scripts.Arc.Domain;

namespace Tickblaze.Scripts.Arc;

public static class SeriesExtensions
{
	public static ISeries<TItem> AsSeries<TItem>(this IReadOnlyList<TItem> items)
	{
		return new ReadOnlySeries<TItem>(items);
	}

	public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
	{
		return new SeriesTransform<TSource, TDestination>(sourceSeries, selector);
	}
}
