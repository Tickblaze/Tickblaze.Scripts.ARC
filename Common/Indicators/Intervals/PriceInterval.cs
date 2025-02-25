namespace Tickblaze.Community;

public class PriceInterval : Interval, IXPositionable<PriceInterval>
{
	public double Price { get; set; } = double.NaN;

	public Point StartPoint => new(StartBarIndex, Price);

	public Point EndPoint => new(EndBarIndex, Price);
}
