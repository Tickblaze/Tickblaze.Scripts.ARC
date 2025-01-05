using System.Diagnostics;

namespace Tickblaze.Scripts.Arc.Core;

public static class EnumerationExtensions
{
	public static bool EnumEquals<TEnum>(this TEnum firstEnum, TEnum secondEnum)
		where TEnum : Enum
	{
		return EqualityComparer<TEnum>.Default.Equals(firstEnum, secondEnum);
	}

	public static int EnumCompareTo<TEnum>(this TEnum firstEnum, TEnum secondEnum)
		where TEnum : Enum
	{
		return Comparer<TEnum>.Default.Compare(firstEnum, secondEnum);
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
