namespace Tickblaze.Scripts.Arc.Domain;

public static class ComponentExtensions
{
	public static bool Intersects(this IBoundable firstBoundable, IBoundable secondBoundable)
	{
		ArgumentNullException.ThrowIfNull(firstBoundable);
		ArgumentNullException.ThrowIfNull(secondBoundable);

		return firstBoundable.Intersects(secondBoundable.Boundary);
	}

	public static bool Intersects(this IBoundable boundable, Rectangle rectangle)
	{
		ArgumentNullException.ThrowIfNull(boundable);

		return boundable.Boundary.Intersects(rectangle);
	}
}
