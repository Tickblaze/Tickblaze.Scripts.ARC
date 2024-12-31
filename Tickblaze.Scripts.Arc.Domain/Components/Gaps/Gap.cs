namespace Tickblaze.Scripts.Arc.Domain;

public sealed class Gap
{
    public bool IsResistance => !IsSupport;

    public required bool IsSupport { get; init; }
    
	public int? EndBarIndex { get; set; }

    public required double EndPrice { get; init; }

    public required int StartBarIndex { get; init; }

    public required double StartPrice { get; init; }
}
