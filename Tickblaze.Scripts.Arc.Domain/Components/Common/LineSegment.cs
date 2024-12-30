namespace Tickblaze.Scripts.Arc.Domain;

public readonly record struct LineSegment : IBoundable
{
    public required Point FromPoint { get; init; }

    public required Point ToPoint { get; init; }

    public Rectangle Boundary => new(FromPoint, ToPoint);
}
