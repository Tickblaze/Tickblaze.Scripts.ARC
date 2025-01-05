namespace Tickblaze.Scripts.Arc.Core;

public interface IDrawingPart<TDrawingPartKey> : IBoundable
	where TDrawingPartKey : IEquatable<TDrawingPartKey>
{
	public TDrawingPartKey Key { get; }
}
