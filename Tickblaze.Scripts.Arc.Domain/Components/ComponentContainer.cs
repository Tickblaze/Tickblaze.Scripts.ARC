using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Domain;

public class ComponentContainer<TComponentKey, TComponent> : IEnumerable<TComponent>
	where TComponentKey : notnull, IEquatable<TComponentKey>
	where TComponent : IBoundable, IComponent<TComponentKey>
{
	protected readonly OrderedDictionary<TComponentKey, TComponent> _components = [];

    public int Count => _components.Count;

    public bool IsEmpty => _components.Count is 0;

	public TComponent LastComponent => this[^1];

	public TComponent this[Index index] => GetComponentAt(index);

	public bool Contains(TComponent component)
	{
		return _components.ContainsKey(component.ComponentKey);
	}

	public bool TryGetComponent(TComponentKey componentKey, [MaybeNullWhen(false)] out TComponent component)
	{
		return _components.TryGetValue(componentKey, out component);
	}

	public TComponent GetComponentAt(Index index)
	{
		var indexOffset = index.GetOffset(_components.Count);

		return _components.GetValueAt(indexOffset);
	}

	public virtual IEnumerable<TComponent> GetVisibleComponents(Rectangle visibleBoundary)
    {
        var componentIndex = _components.Values
			.Map(component => component.Boundary.EndBarIndex)
			.BinarySearch(visibleBoundary.StartBarIndex);

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
	
	public int IndexOf(TComponentKey componentKey)
	{
		return _components.IndexOf(componentKey);
	}

	private void Reupdate(TComponentKey componentKey)
	{
		if (!_components.TryGetValue(componentKey, out var component))
		{
			throw new InvalidOperationException(nameof(Reupdate));
		}

		Upsert(component);
	}

	public void Upsert(TComponent component)
    {
		Remove(component);

		var insertionIndex = _components.Values
			.Map(component => component.Boundary.EndBarIndex)
			.BinarySearch(component.Boundary.EndBarIndex);

        if (insertionIndex < 0)
        {
            insertionIndex = ~insertionIndex;
        }

        _components.Insert(insertionIndex, component.ComponentKey, component);

		component.BoundaryChanged += Reupdate;
	}

	public bool Remove(TComponent component)
    {
		component.BoundaryChanged -= Reupdate;

		return _components.Remove(component.ComponentKey);
    }

	public bool Remove(TComponentKey componentKey)
	{
		return TryGetComponent(componentKey, out var component) && Remove(component);
	}

	public void RemoveAt(int index)
	{
		var component = _components.GetValueAt(index);

		Remove(component);
	}

	public void Clear()
    {
		foreach (var component in _components.Values)
		{
			component.BoundaryChanged -= Reupdate;
		}

        _components.Clear();
    }

    public IEnumerator<TComponent> GetEnumerator()
    {
		return _components.Values.GetEnumerator();
	}

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
    }
}
