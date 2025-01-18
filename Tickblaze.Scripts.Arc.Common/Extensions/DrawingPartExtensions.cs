namespace Tickblaze.Scripts.Arc.Common;

public static class DrawingPartExtensions
{
	public static Point GetPoint(this ISeries<double> series, int barIndex)
	{
		return new Point
		{
			BarIndex = barIndex,
			Price = series[barIndex],
		};
	}

	public static DrawingPart<TBoundable> ToDrawingPart<TBoundable>(this TBoundable boundable)
		where TBoundable : IBoundable, IXPositionable<TBoundable>, IEquatable<TBoundable>
	{
		ArgumentNullException.ThrowIfNull(boundable);

		return new DrawingPart<TBoundable>
		{
			Key = boundable
		};
	}

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
