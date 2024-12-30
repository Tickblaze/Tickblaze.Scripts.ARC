namespace Tickblaze.Scripts.Arc.Domain;

public static class DictionaryExtensions
{
	public static TValue GetValueAt<TKey, TValue>(this OrderedDictionary<TKey, TValue> orderedDictionary, int index)
		where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(orderedDictionary);

		var (_, value) = orderedDictionary.GetAt(index);

		return value;
	}

	public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
		where TKey : notnull
	{
		var dictionary = values.ToDictionary(keySelector);

		return new(dictionary);
	}

	public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params ReadOnlySpan<TKey> keys)
	{
		ArgumentNullException.ThrowIfNull(dictionary);

		foreach (var key in keys)
		{
			dictionary.Remove(key);
		}
	}

	public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
	{
		ArgumentNullException.ThrowIfNull(dictionary);

		foreach (var key in keys)
		{
			dictionary.Remove(key);
		}
	}
}
