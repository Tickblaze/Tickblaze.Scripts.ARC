namespace Tickblaze.Scripts.Arc.Domain;

public class LineSegment : IBoundable
{
    public Rectangle Boundary => new(FromPoint, ToPoint);

    public required Point FromPoint { get; init; }

    public required Point ToPoint { get; init; }
}
