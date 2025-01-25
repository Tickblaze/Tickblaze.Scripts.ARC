using System.Diagnostics;

namespace Tickblaze.Scripts.Arc.Common;

public static class EnumerationExtensions
{
	public static int ToSignum(this Trend trend)
	{
		return trend switch
		{
			Trend.Down => -1,
			Trend.None => 0,
			Trend.Up => 1,
			_ => throw new UnreachableException(),
		};
	}

	public static Trend ToTrend(this int @int)
	{
		return @int.CompareTo(0) switch
		{
			> 0 => Trend.Up,
			0 => Trend.None,
			< 0 => Trend.Down,
		};
	}

	public static Trend ToTrend(this double @double)
	{
		return @double.ApproxCompareTo(0).ToTrend();
	}

	public static StrictTrend ToStrictTrend(this Trend trend)
	{
		return trend switch
		{
			Trend.Up => StrictTrend.Up,
			Trend.Down => StrictTrend.Down,
			_ => throw new ArgumentException(string.Empty, nameof(trend)),
		};
	}

	public static StrictTrend GetOppositeTrend(this StrictTrend strictTrend)
	{
		return strictTrend.Map(StrictTrend.Down, StrictTrend.Up);
	}

	public static TResult Map<TResult>(this StrictTrend strictTrend, TResult upTrendResult, TResult downTrendResult)
	{
		return strictTrend switch
		{
			StrictTrend.Up => upTrendResult,
			StrictTrend.Down => downTrendResult,
			_ => throw new UnreachableException(),
		};
	}	

	public static int GetSign(this HorizontalDirection horizontalDirection)
	{
		return horizontalDirection switch
		{
			HorizontalDirection.Right => 1,
			HorizontalDirection.Left => -1,
			_ => throw new UnreachableException(),
		};
	}
}
