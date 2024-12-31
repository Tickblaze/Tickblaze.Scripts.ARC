namespace Tickblaze.Scripts.Arc.Domain;

public interface IComponent<TComponentKey> : IBoundable
	where TComponentKey : IEquatable<TComponentKey>
{
	public TComponentKey ComponentKey { get; }

	public event Action<TComponentKey> BoundaryChanged;
}
