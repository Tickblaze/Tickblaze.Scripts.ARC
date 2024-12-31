namespace Tickblaze.Scripts.Arc.Domain;

public class SwingLine : IComponent<Point>
{
	public Point ComponentKey => StartPoint;

	public event Action<Point> BoundaryChanged = delegate { };

	public Rectangle Boundary => new(StartPoint, EndPoint);

	public SwingLabel Label { get; set; } = SwingLabel.None;
	
	public required StrictTrend Trend { get; init; }

	public required Point StartPoint { get; init; }

	public required Point EndPoint
	{
		get;
		set
		{
			field = value;

			BoundaryChanged(ComponentKey);					
		}
	}
}
