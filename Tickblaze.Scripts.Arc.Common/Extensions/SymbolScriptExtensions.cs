namespace Tickblaze.Scripts.Arc.Common;

public static class SymbolScriptExtensions
{
    public static bool IsNewSession(this SymbolScript symbolScript, int barIndex)
    {
        ArgumentNullException.ThrowIfNull(symbolScript);

        var bars = symbolScript.Bars;
        var exchangeCalendar = bars.Symbol.ExchangeCalendar;

        if (barIndex is 0
            || bars[barIndex] is var currentBar && currentBar is null
            || bars[barIndex - 1] is var previousBar && previousBar is null)
        {
            return false;
        }

        var currentSession = exchangeCalendar.GetSession(currentBar.Time);
        var previousSession = exchangeCalendar.GetSession(previousBar.Time);

        return !Nullable.Equals(previousSession?.StartUtcDateTime, currentSession?.StartUtcDateTime);
    }

    public static bool IsSteppedThrough(this SymbolScript symbolScript, int barIndex, DateTime timeUtc)
    {
        ArgumentNullException.ThrowIfNull(symbolScript);

        var bars = symbolScript.Bars;

        if (barIndex is 0
            || bars[barIndex] is var currentBar && currentBar is null
            || bars[barIndex - 1] is var previousBar && previousBar is null)
        {
            return false;
        }

        return previousBar.Time < timeUtc && currentBar.Time >= timeUtc;
    }
}
