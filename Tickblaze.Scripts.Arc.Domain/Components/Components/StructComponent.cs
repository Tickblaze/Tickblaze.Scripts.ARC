namespace Tickblaze.Scripts.Arc.Domain;

public sealed class StructComponent<TComponent> : IComponent<TComponent>
    where TComponent : struct, IBoundable, IEquatable<TComponent>
{
    public StructComponent(TComponent component)
    {
        Component = component;
    }

    public Rectangle Boundary => Component.Boundary;

	public TComponent ComponentKey => Component;

	public required TComponent Component { get; init; }

	public event Action<TComponent> BoundaryChanged = delegate { };
}
