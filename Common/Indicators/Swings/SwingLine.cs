namespace Tickblaze.Community;

public sealed class SwingLine : IDrawingPart<Point>, IXPositionable<SwingLine>
{
    public Point Key => StartPoint;

	public static bool IsSequential => true;

	public required SwingLabel Label { get; set; } = SwingLabel.None;
	
	public required StrictTrend Trend { get; init; }

	public required Point StartPoint { get; init; }

	public required Point EndPoint { get; set; }

	public double StartPrice => StartPoint.Price;

	public double EndPrice => EndPoint.Price;

	public Rectangle Boundary => new(StartPoint, EndPoint);
}
