using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public class DrawingPartDictionary<TDrawingPartKey, TDrawingPart> : IReadOnlyList<TDrawingPart>
	where TDrawingPartKey : notnull, IEquatable<TDrawingPartKey>
	where TDrawingPart : IDrawingPart<TDrawingPartKey>
{
	protected readonly OrderedDictionary<TDrawingPartKey, TDrawingPart> _drawingParts = [];

    public int Count => _drawingParts.Count;

    public bool IsEmpty => _drawingParts.Count is 0;

	public TDrawingPart LastDrawingPart => this[^1];

    public TDrawingPart this[int index] => GetDrawingPartAt(index);

    public TDrawingPart this[Index index] => GetDrawingPartAt(index);

	public bool Contains(TDrawingPart drawingPart)
	{
		return _drawingParts.ContainsKey(drawingPart.Key);
	}

	public bool TryGetDrawingPart(TDrawingPartKey componentKey, [MaybeNullWhen(false)] out TDrawingPart drawingPart)
	{
		return _drawingParts.TryGetValue(componentKey, out drawingPart);
	}

	public TDrawingPart GetDrawingPartAt(Index index)
	{
		var indexOffset = index.GetOffset(_drawingParts.Count);

		return _drawingParts.GetValueAt(indexOffset);
	}

	public virtual IEnumerable<TDrawingPart> GetVisibleDrawingParts(Rectangle visibleBoundary)
    {
        var componentIndex = _drawingParts.Values
			.AsSeries()
			.Map(component => component.Boundary.StartBarIndex)
			.BinarySearch(visibleBoundary.EndBarIndex);

		// Todo: redo.

		if (componentIndex < 0)
        {
            componentIndex = ~componentIndex;
        }

		var currentIndex = componentIndex;

		while (currentIndex < Count
			&& this[currentIndex] is var component
			&& component.Boundary is var boundary
			&& boundary.StartBarIndex <= visibleBoundary.EndBarIndex)
		{
			if (component.Intersects(visibleBoundary))
			{
				yield return component;
			}

			currentIndex++;
		}

		currentIndex = componentIndex - 1;

		while (currentIndex >= 0
			&& this[currentIndex] is var component
			&& component.Boundary is var boundary
			&& boundary.EndBarIndex >= visibleBoundary.StartBarIndex)
		{
			if (component.Intersects(visibleBoundary))
			{
				yield return component;
			}

			currentIndex--;
		}
    }
	
	public int IndexOf(TDrawingPartKey componentKey)
	{
		return _drawingParts.IndexOf(componentKey);
	}

	public void AddOrUpdate(TDrawingPart drawingPart)
    {
		var drawingPartKey = drawingPart.Key;
		var startBarIndex = drawingPart.Boundary.StartBarIndex;
		var drwaingPartIndex = IndexOf(drawingPartKey);

		if (drwaingPartIndex is not -1)
		{
			var cachedDrawingPart = GetDrawingPartAt(drwaingPartIndex);

			if (!startBarIndex.Equals(cachedDrawingPart.Boundary.StartBarIndex))
			{
				throw new ArgumentException(string.Empty, nameof(drawingPart));
			}

			_drawingParts.SetAt(drwaingPartIndex, drawingPart);

			return;
		}

		var insertionIndex = _drawingParts.Values
			.AsSeries()
			.Map(component => component.Boundary.StartBarIndex)
			.BinarySearch(startBarIndex);

        if (insertionIndex < 0)
        {
            insertionIndex = ~insertionIndex;
        }

		while (insertionIndex + 1 < Count
			&& this[insertionIndex + 1] is var nextDrawingPart
			&& nextDrawingPart.Boundary is var boundary
			&& startBarIndex.Equals(boundary.StartBarIndex))
		{
			insertionIndex++;
		}

        _drawingParts.Insert(insertionIndex, drawingPartKey, drawingPart);
	}

	public bool Remove(TDrawingPart component)
    {
		return _drawingParts.Remove(component.Key);
    }

	public bool Remove(TDrawingPartKey componentKey)
	{
		return TryGetDrawingPart(componentKey, out var component) && Remove(component);
	}

	public void RemoveAt(int index)
	{
		var component = _drawingParts.GetValueAt(index);

		Remove(component);
	}

	public void Clear()
    {
        _drawingParts.Clear();
    }

    public IEnumerator<TDrawingPart> GetEnumerator()
    {
		return _drawingParts.Values.GetEnumerator();
	}

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
    }
}
