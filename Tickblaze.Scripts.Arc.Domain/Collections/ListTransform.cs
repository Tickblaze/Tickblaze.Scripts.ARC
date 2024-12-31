using System.Collections;

namespace Tickblaze.Scripts.Arc.Domain;

public class ListTransform<TSource, TDestination> : IReadOnlyList<TDestination>
{
    public ListTransform(IReadOnlyList<TSource> sourceList, Func<TSource, TDestination> selector)
    {
        _selector = selector;
        _sourceList = sourceList;
    }

    private readonly IReadOnlyList<TSource> _sourceList;
    private readonly Func<TSource, TDestination> _selector;

	public int Count => _sourceList.Count;

    public TDestination this[int index] => _selector(_sourceList[index]);

    public IEnumerator<TDestination> GetEnumerator()
    {
		for (var index = 0; index < _sourceList.Count; index++)
		{
			yield return this[index];
		}
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
	}
}
