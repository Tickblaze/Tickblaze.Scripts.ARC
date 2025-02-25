namespace Tickblaze.Community;

public static class DictionaryExtensions
{
	public static TValue GetValueAt<TKey, TValue>(this OrderedDictionary<TKey, TValue> orderedDictionary, int index)
		where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(orderedDictionary);

		var (_, value) = orderedDictionary.GetAt(index);

		return value;
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
