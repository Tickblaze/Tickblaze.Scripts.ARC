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

	public static TItem GetAtOrDefault<TItem>(this ISeries<TItem> items, Index index, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		var offset = index.GetOffset(items.Count);

		if (0 <= offset && offset < items.Count)
		{
			return items.GetAt(offset);
		}

		return defaultItem;
	}

	public static TItem GetLastOrDefault<TItem>(this ISeries<TItem> items, TItem defaultItem)
	{
		return items.GetAtOrDefault(^1, defaultItem);
	}

	public static ISeries<TItem> AsSeries<TItem>(this IReadOnlyList<TItem> items)
    {
        return new ReadOnlySeries<TItem>(items);
    }

    public static ISeries<TDestination> Map<TSource, TDestination>(this ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
    {
        return new SeriesTransform<TSource, TDestination>(sourceSeries, selector);
    }
}
