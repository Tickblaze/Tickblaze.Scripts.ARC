namespace Tickblaze.Scripts.Arc.Common;

public class Interval : IDrawingPart<int>
{
	public int Key => StartBarIndex;

	public Rectangle Boundary => new()
	{
		EndBarIndex = EndBarIndex,
		EndPrice = double.MaxValue,
		StartPrice = double.MinValue,
		StartBarIndex = StartBarIndex,
	};

	public required int EndBarIndex { get; set; }

	public required int StartBarIndex { get; init; }
}
