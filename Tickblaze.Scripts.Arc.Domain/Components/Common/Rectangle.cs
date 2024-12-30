using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Domain;

public class Rectangle
{
    public Rectangle()
    {
        
    }

    [SetsRequiredMembers]
    public Rectangle(Point fromPoint, Point toPoint)
    {
        (FromBarIndex, FromPrice) = fromPoint;

        (ToBarIndex, ToPrice) = toPoint;
    }

    public required int FromBarIndex { get; init; }

	public required double FromPrice { get; init; }

	public required int ToBarIndex { get; init; }

	public required double ToPrice { get; init; }
    
    public Point TopRight => new(ToBarIndex, ToPrice);
    
    public Point TopLeft => new(FromBarIndex, ToPrice);

    public Point BottomRight => new(ToBarIndex, FromPrice);
    
    public Point BottomLeft => new(FromBarIndex, FromPrice);

    public bool IsEmpty => FromBarIndex.Equals(ToBarIndex) && FromPrice.Equals(ToPrice);

	public bool Contains(Point point, bool areBoundsIncluded = true)
	{
		return areBoundsIncluded
			? FromBarIndex <= point.BarIndex && point.BarIndex <= ToBarIndex
				&& FromPrice <= point.Price && point.Price <= ToPrice
			: FromBarIndex < point.BarIndex && point.BarIndex < ToBarIndex
				&& FromPrice < point.Price && point.Price < ToPrice;
	}

    public bool Intersects(Rectangle rectangle)
    {
        return FromBarIndex <= rectangle.ToBarIndex
            && FromPrice <= rectangle.ToPrice
            && rectangle.FromBarIndex <= ToBarIndex
            && rectangle.FromPrice <= ToPrice;
    }
}
