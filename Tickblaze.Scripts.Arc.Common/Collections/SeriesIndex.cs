using System.Collections;

namespace Tickblaze.Scripts.Arc.Common;

public class SeriesIndex<TItem> : ISeries<int>
{
    public SeriesIndex(ISeries<TItem> series)
    {
        _series = series;
    }

    private readonly ISeries<TItem> _series;

	public int Count => _series.Count;

    public int this[int index] => index;

    public int Last(int barsAgo = 0)
    {
		return Count - 1 - barsAgo;
	}

    public IEnumerator<int> GetEnumerator()
    {
		return Enumerable
			.Range(0, Count)
			.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();

	}
}
