namespace Tickblaze.Community;

public class DrawingPart<TBoundable>
	: IDrawingPart<TBoundable>
	, IXPositionable<DrawingPart<TBoundable>>
	where TBoundable : IBoundable, IXPositionable<TBoundable>, IEquatable<TBoundable>
{
    public virtual Rectangle Boundary => Key.Boundary;
	
	public required TBoundable Key { get; init; }

	public static bool IsSequential => TBoundable.IsSequential;
}
