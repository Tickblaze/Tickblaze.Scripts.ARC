namespace Tickblaze.Scripts.Arc.Common;

public sealed class FloodingInterval : TrendInterval, IXPositionable<FloodingInterval>
{
	public required Color Color { get; init; }
}
