namespace Tickblaze.Scripts.Arc.Common;

public static class SeriesExtensions
{
    public static TItem GetAt<TItem>(this ISeries<TItem> items, int index)
    {
		ArgumentNullException.ThrowIfNull(items);

		var item = items[index];

        if (item is null)
        {
            throw new NullReferenceException(nameof(item));
        }

        return item;
    }

	public static TItem GetAtOrDefault<TItem>(this ISeries<TItem> items, int index, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		if (0 <= index && index < items.Count)
		{
			return items.GetAt(index);
		}

		return defaultItem;
	}

	public static TItem GetAtOrDefault<TItem>(this ISeries<TItem> items, Index index, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		var offset = index.GetOffset(items.Count);

		return items.GetAtOrDefault(offset, defaultItem);
	}

	public static TItem GetLastOrDefault<TItem>(this ISeries<TItem> items, TItem defaultItem)
	{
		return items.GetAtOrDefault(^1, defaultItem);
	}

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

	public static ISeries<TItem> AsSeries<TItem>(this IReadOnlyList<TItem> items)
    {
        return new ReadOnlySeries<TItem>(items);
    }

    public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
    {
        return new SeriesTransform<TSource, TDestination>(sourceSeries, selector);
    }

	public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<int, TDestination> selector)
	{
		return new SeriesIndex<TSource>(sourceSeries).Map(selector);
	}

	public static ISeries<TDestination> MapAndCache<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
	{
		return new SeriesTransformCached<TSource, TDestination>(sourceSeries, selector);
	}

	public static ISeries<TDestination> MapAndCacheByIndex<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<int, TDestination> selector)
	{
		return new SeriesIndex<TSource>(sourceSeries).MapAndCache(selector);
	}
}
