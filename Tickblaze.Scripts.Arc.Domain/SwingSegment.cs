namespace Tickblaze.Scripts.Arc.Domain;

public class SwingSegment : IBoundable
{
	public required Point ToPoint { get; set; }
	
	public required Point FromPoint { get; init; }

    public required StrictTrend Trend { get; init; }

	public SwingLabel FromLabel { get; init; } = SwingLabel.None;

	public Rectangle Boundary => new(FromPoint, ToPoint);
}
