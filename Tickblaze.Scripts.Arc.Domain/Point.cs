using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Domain;

public class Point
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

    public required int BarIndex { get; init; }

    public required double Price { get; init; }

}
