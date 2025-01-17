using System.Collections;

namespace Tickblaze.Scripts.Arc.Common;

public class SeriesTransformCached<TSource, TDestination> : ISeries<TDestination>
{
    public SeriesTransformCached(ISeries<TSource> sourceSeries, Func<TSource, TDestination> selector)
    {
        _selector = (index, source) => selector(source);

		_sourceSeries = sourceSeries;
    }

	public SeriesTransformCached(ISeries<TSource> sourceSeries, Func<int, TSource, TDestination> selector)
	{
		_selector = selector;

		_sourceSeries = sourceSeries;
	}

	private readonly ISeries<TSource> _sourceSeries;
    
	private readonly Func<int, TSource, TDestination> _selector;
	
	private readonly Series<Lazy<TDestination?>> destinationSeries = [];

	public int Count => _sourceSeries.Count;
    
	public TDestination? this[int index] => GetDestination(index);

    private TDestination? GetDestination(int index)
    {
		var destination = destinationSeries[index] ??= new Lazy<TDestination?>(GetDestinationValue);

		return destination.Value;

		TDestination? GetDestinationValue()
		{
			var sourceValue = _sourceSeries[index];

            return sourceValue is null ? default : _selector(index, sourceValue);
		}
	}
    
	public IEnumerator<TDestination?> GetEnumerator()
    {
		foreach (int index in Enumerable.Range(0, Count))
		{
			yield return this[index];
		}
	}

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
    }
}
