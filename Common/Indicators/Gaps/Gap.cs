namespace Tickblaze.Community;

public sealed class Gap : IDrawingPart<int>, IXPositionable<Gap>
{
    public int Key => StartBarIndex;

	public static bool IsSequential => false;

	public bool IsResistance => !IsSupport;

    public required bool IsSupport { get; init; }
    
	public int? EndBarIndex { get; set; }

    public required double EndPrice { get; init; }

    public required int StartBarIndex { get; init; }

    public required double StartPrice { get; init; }

	public bool IsEmpty => Nullable.Equals(StartBarIndex, EndBarIndex);

	public Rectangle Boundary => new()
    {
		EndPrice = EndPrice,
		StartPrice = StartPrice,
		StartBarIndex = StartBarIndex,
		EndBarIndex = EndBarIndex ?? int.MaxValue,
	};
}
