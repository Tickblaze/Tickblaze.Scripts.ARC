namespace Tickblaze.Scripts.Arc.Common;

public sealed class SwingLine : IDrawingPart<Point>
{
	public Point Key => StartPoint;

	public required SwingLabel Label { get; set; } = SwingLabel.None;
	
	public required StrictTrend Trend { get; init; }

	public required Point StartPoint { get; init; }

	public required Point EndPoint { get; set; }

	public Rectangle Boundary => new(StartPoint, EndPoint);
}
