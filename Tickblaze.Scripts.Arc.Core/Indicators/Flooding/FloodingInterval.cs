namespace Tickblaze.Scripts.Arc.Core;

public sealed class FloodingInterval : IDrawingPart<int> 
{
	public int Key => StartBarIndex;

	public Rectangle Boundary => new()
	{
		EndBarIndex = EndBarIndex,
		EndPrice = double.MinValue,
		StartPrice = double.MaxValue,
		StartBarIndex = StartBarIndex,
	};
	
	public required Color Color { get; init; }

	public required int EndBarIndex { get; set; }

	public required StrictTrend Trend { get; init; }

	public required int StartBarIndex { get; init; }
}
