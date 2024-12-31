using System.ComponentModel.DataAnnotations;

namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	[NumericRange(MinValue = 1)]
	[Parameter("BB Period", GroupName = "MACDBB Parameters", Description = "Band period for Bollinger Bands")]
	public int BbPeriod { get; set; } = 10;

	[NumericRange(MaxValue = 100)]
	[Parameter("BB Channel Opacity", GroupName = "MACDBB Parameters", Description = "Opacity for shading the area between the Bollinger Bands")]
	public int BbChannelOpacity { get; set; } = 20;

	[Parameter("BB Channel Color", GroupName = "MACDBB Parameters")]
	public Color BbChannelColor { get; set; } = DrawingColor.DodgerBlue;

	[Plot("BB Average")]
	public PlotSeries BbAverage { get; set; } = new(Color.Transparent, LineStyle.Dash, 2);

	[Plot("BB Upper Band")]
	public PlotSeries BbUpperBand { get; set; } = new(Color.Black, LineStyle.Solid, 2);

	[Plot("BB Lower Band")]
	public PlotSeries BbLowerBand { get; set; } = new(Color.Black, LineStyle.Solid, 2);

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Fast EMA Period", GroupName = "MACDBB Parameters", Description = "Period for fast EMA")]
	public int MacdFastEmaPeriod { get; set; } = 12;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Slow EMA Period", GroupName = "MACDBB Parameters", Description = "Period for slow EMA")]
	public int MacdSlowEmaPeriod { get; set; } = 26;

	// Todo: description.
	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("MACD Std. Dev. Multiplier", GroupName = "MACDBB Parameters", Description = "Number of standard deviations")]
	public double StdDevMultiplier { get; set; } = 1.0;

	[NumericRange(MinValue = 1)]
	[Display(Name = "MACD Dot Size", GroupName = "MACDBB Parameters", Description = "Size of MACD dots", Order = 0)]
	public int MacdDotSize { get; set; } = 2;

	[Parameter("MACD Dots Rim Color")]
	public Color MacdDotRimColor { get; set; } = Color.Black;

	[Parameter("MACD Rising Dots Above Channel Color")]
	public Color MacdRisingAboveChannelDotColor { get; set; } = Color.Green;

	[Parameter("MACD Rising Dots Inside/Below Channel Color")]
	public Color MacdRisingBelowChannelDotColor { get; set; } = Color.Green;

	[Parameter("MACD Falling Dots Above Channel Color")]
	public Color MacdFallingAboveChannelDotColor { get; set; } = Color.Red;

	[Parameter("MACD Falling Dots Inside/Below Channel Color")]
	public Color MacdFallingBelowChannelDotColor { get; set; } = Color.Red;

	[Plot("MACD Connector")]
	public PlotSeries MacdConnector { get; set; } = new(Color.White, LineStyle.Solid, 2);
}
