namespace Tickblaze.Scripts.Arc.Common;

public sealed class FloodingInterval : IDrawingPart<int> 
{
	public int Key => StartBarIndex;

	public Rectangle Boundary => new()
	{
		EndBarIndex = EndBarIndex,
		EndPrice = double.MaxValue,
		StartPrice = double.MinValue,
		StartBarIndex = StartBarIndex,
	};
	
	public required Color Color { get; init; }

	public required int EndBarIndex { get; set; }

	public required StrictTrend Trend { get; init; }

	public required int StartBarIndex { get; init; }
}
