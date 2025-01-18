using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public readonly record struct Point : IBoundable, IXPositionable<Point>
{
    public Point()
    {
        
    }

    [SetsRequiredMembers]
    public Point(int barIndex, double price)
    {
        BarIndex = barIndex;
        
        Price = price;
    }

	public static bool IsSequential => true;

    public Rectangle Boundary => new(this, this);

    public required int BarIndex { get; init; }

    public required double Price { get; init; }

    public void Deconstruct(out int barIndex, out double price)
    {
        barIndex = BarIndex;

        price = Price;
    }
}
