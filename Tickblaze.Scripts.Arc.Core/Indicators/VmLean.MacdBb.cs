using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	private Macd Macd => _vmLeanCore.Macd;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.5)]
	[Parameter("Bollinger Bands Std. Dev. Multiplier", GroupName = "MACDBB Parameters", Description = "Std. dev. multiplier of the Bollinger Bands")]
	public double BandMultiplier { get; set; } = 1.0;
	
	[NumericRange(MinValue = 1)]
	[Parameter("Bollinger Bands Period", GroupName = "MACDBB Parameters", Description = "Period of the Bollinger Bands")]
	public int BandPeriod { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Fast EMA Period", GroupName = "MACDBB Parameters", Description = "Period of the fast MACD EMA")]
	public int MacdFastPeriod { get; set; } = 12;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Slow EMA Period", GroupName = "MACDBB Parameters", Description = "Period of the slow MACD EMA")]
	public int MacdSlowPeriod { get; set; } = 26;

	[Parameter("Bollinger Bands Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the Bollinger Bands channel")]
	public Color BbChannelColor { get; set; } = DrawingColor.DodgerBlue.With(opacity: 0.2f);

	[Parameter("MACD Rising Dots Above Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an uptrend when they are above the Bollinger Bands channel")]
	public Color MacdRisingAboveChannelDotColor { get; set; } = Color.White;

	[Parameter("MACD Rising Dots Inside/Below Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an uptrend when they are inside/below the Bollinger Bands channel")]
	public Color MacdRisingBelowChannelDotColor { get; set; } = DrawingColor.Lime;

	[Parameter("MACD Falling Dots Inside/Above Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an downtrend when they are inside/above the Bollinger Bands channel")]
	public Color MacdFallingAboveChannelDotColor { get; set; } = Color.Red;

	[Parameter("MACD Falling Dots Below Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an downtrend when they are below the Bollinger Bands channel")]
	public Color MacdFallingBelowChannelDotColor { get; set; } = Color.Black;

	[Plot("Band Average")]
	public PlotSeries BandAverage { get; set; } = new(Color.Transparent, LineStyle.Dot, 3);

	[Plot("Band Lower")]
	public PlotSeries BandLower { get; set; } = new(Color.Black, LineStyle.Solid, 3);
	
	[Plot("Band Upper")]
	public PlotSeries BandUpper { get; set; } = new(Color.Black, LineStyle.Solid, 3);

	[Plot("MACD Connector")]
	public PlotSeries MacdConnector { get; set; } = new(DrawingColor.CornflowerBlue, LineStyle.Solid, 8);

	[Plot("MACD Dots")]
	public PlotSeries MacdDots { get; set; } = new(Color.Transparent, PlotStyle.Dot, 6);

	private void InitializeMacdBb()
	{
		ShadeBetween(BandLower, BandUpper, default, BbChannelColor, BbChannelColor.GetOpacity());
	}

	private void CalculateMacdBb(int barIndex)
	{
		var currentValue = MacdConnector[barIndex] = MacdDots[barIndex] = Macd.Result[barIndex];
		
		var previousValue = MacdDots.GetAtOrDefault(barIndex - 1, currentValue);

		var upperBandValue = BandUpper[barIndex] = _vmLeanCore.UpperBand[barIndex];
		var lowerBandValue = BandLower[barIndex] = _vmLeanCore.LowerBand[barIndex];

		MacdDots.Colors[barIndex] = currentValue.CompareTo(previousValue) switch
		{
			> 0 when currentValue > upperBandValue => MacdRisingAboveChannelDotColor,
			> 0 => MacdRisingBelowChannelDotColor,
			< 0 when currentValue < lowerBandValue => MacdFallingBelowChannelDotColor,
			< 0 => MacdFallingAboveChannelDotColor,
			_ => MacdDots.Colors.GetAtOrDefault(barIndex - 1, Color.Transparent),
		};
	}
}
