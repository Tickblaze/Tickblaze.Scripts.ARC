using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Domain;

public class ComponentContainer<TComponent> : IEnumerable<TComponent>
    where TComponent : IBoundable
{
    protected readonly OrderedDictionary<int, TComponent> _components = [];

	public int Count => _components.Count;

    public bool IsEmpty => _components.Count is 0;

	public TComponent LastComponent => GetComponentAt(^1);

	public bool TryGetComponent(int barIndex, [NotNullWhen(true)] out TComponent? component)
	{
		return _components.TryGetValue(barIndex, out component);
	}

    public virtual IEnumerable<TComponent> GetVisibleComponents(Rectangle visibleRectangle)
    {
		var fromBarIndex = visibleRectangle.FromBarIndex;

        var componentIndex = _components.Keys.BinarySearch(fromBarIndex);

        if (componentIndex < 0)
        {
            componentIndex = ~componentIndex;
        }

        while (componentIndex < _components.Count)
        {
			var component = _components.GetValueAt(componentIndex);

			if (visibleRectangle.Intersects(component.Boundary))
			{
				yield return _components.GetValueAt(componentIndex);
			}

			componentIndex++;
        }
    }

	public TComponent GetComponentAt(Index index)
	{
		var indexOffset = index.GetOffset(_components.Count);

        return _components.GetValueAt(indexOffset);
	}
	
	public int IndexOf(int barIndex)
	{
		return _components.IndexOf(barIndex);
	}

	public void Upsert(TComponent component)
    {
        var fromBarIndex = component.FromBarIndex;

        var componentIndex = _components.Keys.BinarySearch(fromBarIndex);

        if (componentIndex >= 0)
        {
            _components.SetAt(componentIndex, component);

            return;
        }
        else
        {
            componentIndex = ~componentIndex;

            _components.Insert(componentIndex, fromBarIndex, component);
        }
    }

	public bool Remove(TComponent component)
    {
        return _components.Remove(component.FromBarIndex);
    }

	public bool Remove(int barIndex)
	{
		return _components.Remove(barIndex);
	}

	public void RemoveAt(int index)
	{
		_components.RemoveAt(index);
	}

	public void Clear()
    {
        _components.Clear();
    }

    public IEnumerator<TComponent> GetEnumerator()
    {
		foreach (var (_, component) in _components)
		{
			yield return component;
		}
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
		return GetEnumerator();
    }
}
