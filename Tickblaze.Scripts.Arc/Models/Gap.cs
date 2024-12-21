namespace Tickblaze.Scripts.Arc;

public class Gap
{
    public int? ToIndex { get; set; }

    public bool IsResistance => !IsSupport;

    public required int FromIndex { get; init; }

    public required double TopPrice { get; init; }

    public required double BottomPrice { get; init; }

    public required bool IsSupport { get; init; }
}
