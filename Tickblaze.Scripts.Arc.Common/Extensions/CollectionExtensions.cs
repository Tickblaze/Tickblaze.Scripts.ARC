namespace Tickblaze.Scripts.Arc.Common;

public static class CollectionExtensions
{
	public static TItem GetAt<TItem>(this IReadOnlyList<TItem> items, int index)
	{
		ArgumentNullException.ThrowIfNull(items);

		var item = items[index];

		if (item is null)
		{
			throw new NullReferenceException(nameof(item));
		}

		return item;
	}

	public static TItem GetAtOrDefault<TItem>(this IReadOnlyList<TItem> items, int index, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		if (0 <= index && index < items.Count)
		{
			return items.GetAt(index);
		}

		return defaultItem;
	}

	public static TItem GetAtOrDefault<TItem>(this IReadOnlyList<TItem> items, Index index, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		var offset = index.GetOffset(items.Count);

		return items.GetAtOrDefault(offset, defaultItem);
	}

	public static TItem GetLast<TItem>(this IReadOnlyList<TItem> items)
	{
		ArgumentNullException.ThrowIfNull(items);

		return items.GetAt(items.Count - 1);
	}

	public static TItem GetLastOrDefault<TItem>(this IReadOnlyList<TItem> items, TItem defaultItem)
	{
		return items.GetAtOrDefault(^1, defaultItem);
	}

	public static TItem? GetLastOrDefault<TItem>(this IReadOnlyList<TItem> items)
	{
		return items.GetAtOrDefault(^1, default);
	}

	public static void BackfillAddOrUpdate<TItem>(this IList<TItem> items, int index, TItem item, TItem defaultItem)
	{
		ArgumentNullException.ThrowIfNull(items);

		if (index < 0)
		{
			throw new ArgumentException(string.Empty, nameof(index));
		}

		if (0 <= index && index < items.Count)
		{
			items[index] = item;
		}

		while (items.Count < index)
		{
			items.Add(defaultItem);
		}

		items.Add(item);
	}

	public static ISeries<TItem> AsSeries<TItem>(this IReadOnlyList<TItem> items)
	{
		return new ReadOnlySeries<TItem>(items);
	}
}
