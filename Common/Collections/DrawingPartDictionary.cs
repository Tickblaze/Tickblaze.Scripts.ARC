﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public class DrawingPartDictionary<TDrawingPartKey, TDrawingPart> : ISeries<TDrawingPart>
	where TDrawingPartKey : notnull, IEquatable<TDrawingPartKey>
	where TDrawingPart : IDrawingPart<TDrawingPartKey>, IXPositionable<TDrawingPart>
{
	protected readonly OrderedDictionary<TDrawingPartKey, TDrawingPart> _drawingParts = [];

    public int Count => _drawingParts.Count;

    public bool IsEmpty => _drawingParts.Count is 0;

	public TDrawingPart LastDrawingPart => this[^1];

    public TDrawingPart this[int index] => GetDrawingPartAt(index);

    public TDrawingPart this[Index index] => GetDrawingPartAt(index);

	public bool Contains(TDrawingPart drawingPart)
	{
		return Contains(drawingPart.Key);
	}

	public bool Contains(TDrawingPartKey drawingPartKey)
	{
		return _drawingParts.ContainsKey(drawingPartKey);
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
			.Select(component => component.Boundary.StartBarIndex)
			.BinarySearch(visibleBoundary.EndBarIndex);

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
			&& component.Boundary is var boundary)
		{
			if (TDrawingPart.IsSequential
				&& boundary.EndBarIndex < visibleBoundary.StartBarIndex)
			{
				break;
			}
			
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
		var drawingPartIndex = IndexOf(drawingPartKey);

		if (drawingPartIndex is not -1)
		{
			var cachedDrawingPart = GetDrawingPartAt(drawingPartIndex);

			if (!startBarIndex.Equals(cachedDrawingPart.Boundary.StartBarIndex))
			{
				throw new ArgumentException(string.Empty, nameof(drawingPart));
			}

			_drawingParts.SetAt(drawingPartIndex, drawingPart);

			return;
		}

		var insertionIndex = _drawingParts.Values
			.AsSeries()
			.Select(component => component.Boundary.StartBarIndex)
			.BinarySearch(startBarIndex);

        if (insertionIndex < 0)
        {
            insertionIndex = ~insertionIndex;
        }

		while (insertionIndex < Count
			&& this[insertionIndex] is var nextDrawingPart
			&& nextDrawingPart.Boundary is var boundary
			&& startBarIndex.Equals(boundary.StartBarIndex))
		{
			insertionIndex++;
		}

        _drawingParts.Insert(insertionIndex, drawingPartKey, drawingPart);
	}

	public bool Remove(TDrawingPart drawingPart)
    {
		return _drawingParts.Remove(drawingPart.Key);
    }

	public bool Remove(TDrawingPartKey drawingPartKey)
	{
		return TryGetDrawingPart(drawingPartKey, out var drawingPart) && Remove(drawingPart);
	}

	public void RemoveAt(int index)
	{
		_drawingParts.RemoveAt(index);
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
