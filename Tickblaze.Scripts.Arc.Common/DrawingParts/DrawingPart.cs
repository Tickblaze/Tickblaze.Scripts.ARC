namespace Tickblaze.Scripts.Arc.Common;

public class DrawingPart<TBoundable> : IDrawingPart<TBoundable>
	where TBoundable : IBoundable, IEquatable<TBoundable>
{
	public Rectangle Boundary => Key.Boundary;
	
	public required TBoundable Key { get; init; }
}
