using System.Diagnostics;

namespace Tickblaze.Scripts.Arc.Domain;

public static class TrendExtensions
{
	public static StrictTrend GetOppositeTrend(this StrictTrend strictTrend)
	{
		return strictTrend switch
		{
			StrictTrend.Up => StrictTrend.Down,
			StrictTrend.Down => StrictTrend.Up,
			_ => throw new UnreachableException(),
		};
	}
}
