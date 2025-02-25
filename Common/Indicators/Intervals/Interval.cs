namespace Tickblaze.Community;

public class Interval : IDrawingPart<int>, IXPositionable<Interval>
{
    public int Key => StartBarIndex;

	public static bool IsSequential => true;

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
