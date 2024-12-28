namespace Tickblaze.Scripts.Arc.Domain;

public sealed class ComponentContainer<TComponent>
    where TComponent : IBoundable
{
    private readonly OrderedDictionary<int, TComponent> _components = [];

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
            var (_, component) = _components.GetAt(componentIndex);
            
            yield return component;

            componentIndex++;
        }
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

    public void Clear()
    {
        _components.Clear();
    }
}
