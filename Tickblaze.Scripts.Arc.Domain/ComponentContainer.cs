using System.Runtime.InteropServices;

namespace Tickblaze.Scripts.Arc.Domain;

public sealed class ComponentContainer<TComponent>
    where TComponent : IBoundable
{
    private readonly OrderedDictionary<int, TComponent> _components = [];

	public bool IsEmpty => _components.Count is 0;

	public int Count => _components.Count;

	public TComponent LastComponent => GetComponentAt(^1);

    public IEnumerable<TComponent> GetVisibleComponents(Rectangle visibleRectangle)
    {
		var fromBarIndex = visibleRectangle.FromBarIndex;

        var componentIndex = _components.Keys.BinarySearch(fromBarIndex);

        if (componentIndex < 0)
        {
            componentIndex = ~componentIndex;
        }

        while (componentIndex < _components.Count)
        {
            yield return _components.GetValueAt(componentIndex);

			componentIndex++;
        }
    }

	public TComponent GetComponentAt(Index index)
	{
		var indexOffset = index.GetOffset(_components.Count);

        return _components.GetValueAt(indexOffset);
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

	public int IndexOf(int barIndex)
	{
		return _components.IndexOf(barIndex);
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
}
