namespace Tickblaze.Scripts.Arc.Core;

public sealed class SwingLine : IDrawingPart<DrawingPoint>
{
	public DrawingPoint Key => StartPoint;

	public required SwingLabel Label { get; set; } = SwingLabel.None;
	
	public required StrictTrend Trend { get; init; }

	public required DrawingPoint StartPoint { get; init; }

	public required DrawingPoint EndPoint { get; set; }

	public Rectangle Boundary => new(StartPoint, EndPoint);
}
