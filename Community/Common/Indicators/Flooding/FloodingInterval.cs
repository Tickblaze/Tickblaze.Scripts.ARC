namespace Tickblaze.Community;

public sealed class FloodingInterval : TrendInterval, IXPositionable<FloodingInterval>
{
	public required Color Color { get; init; }
}
