using System.Collections;

namespace Tickblaze.Scripts.Arc.Common;

public class ReadOnlySeries<TItem> : ISeries<TItem>
{
    public ReadOnlySeries(IReadOnlyList<TItem> items)
    {
        _items = items;
    }

    private readonly IReadOnlyList<TItem> _items;

	public int Count => _items.Count;

    public TItem? this[int index] => _items[index];

    public TItem? Last(int barsAgo = 0)
    {
		return _items[^(barsAgo + 1)];
	}
    
	public IEnumerator<TItem?> GetEnumerator()
    {
		return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
	}
}
