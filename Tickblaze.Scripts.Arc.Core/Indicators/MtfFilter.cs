namespace Tickblaze.Scripts.Arc.Core;

public partial class MtfFilter : Indicator
{
	[NumericRange(MinValue = 1)]
	[Parameter("BB Period", GroupName = "MACDBB Parameters", Description = "Period of the Bollinger Bands")]
	public int BbPeriod { get; set; } = 10;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.5)]
	[Parameter("BB Std. Dev. Multiplier", GroupName = "MACDBB Parameters", Description = "Std. dev. multiplier of the Bollinger Bands")]
	public double BbMultiplier { get; set; } = 1.0;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Fast EMA Period", GroupName = "MACDBB Parameters", Description = "Period of the fast MACD EMA")]
	public int MacdFastPeriod { get; set; } = 12;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Slow EMA Period", GroupName = "MACDBB Parameters", Description = "Period of the slow MACD EMA")]
	public int MacdSlowPeriod { get; set; } = 26;
}
