using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Core;

public readonly record struct Rectangle
{
	public Rectangle()
	{
		
	}

	[SetsRequiredMembers]
	public Rectangle(Point firstPoint, Point secondPoint)
	{
		StartBarIndex = Math.Min(firstPoint.BarIndex, secondPoint.BarIndex);
		StartPrice = Math.Min(firstPoint.Price, secondPoint.Price);

		EndBarIndex = Math.Max(firstPoint.BarIndex, secondPoint.BarIndex);
		EndPrice = Math.Max(firstPoint.Price, secondPoint.Price);
	}

	[SetsRequiredMembers]
	public Rectangle(int firstBarIndex, double firstPrice, int secondIndex, double secondPrice)
    {
        StartBarIndex = firstBarIndex;
        StartPrice = firstPrice;

        EndBarIndex = secondIndex;
        EndPrice = secondPrice;
    }

    public required int StartBarIndex { get; init; }

	public required double StartPrice { get; init; }

	public required int EndBarIndex { get; init; }

	public required double EndPrice { get; init; }
	
	public Point TopRight => new(EndBarIndex, EndPrice);
	
	public Point TopLeft => new(StartBarIndex, EndPrice);

	public Point BottomRight => new(EndBarIndex, StartPrice);
	
	public Point BottomLeft => new(StartBarIndex, StartPrice);

	public bool IsEmpty => StartBarIndex.Equals(EndBarIndex) && StartPrice.Equals(EndPrice);

	public bool Contains(Point point, bool areBoundsIncluded = true)
	{
		return areBoundsIncluded
			? StartBarIndex <= point.BarIndex && point.BarIndex <= EndBarIndex
				&& StartPrice <= point.Price && point.Price <= EndPrice
			: StartBarIndex < point.BarIndex && point.BarIndex < EndBarIndex
				&& StartPrice < point.Price && point.Price < EndPrice;
	}

	public bool Intersects(Rectangle rectangle)
	{
		return Math.Max(StartBarIndex, rectangle.StartBarIndex) <= Math.Min(EndBarIndex, rectangle.EndBarIndex)
			& Math.Max(StartPrice, rectangle.StartPrice) <= Math.Min(EndPrice, rectangle.EndPrice);
	}
}
