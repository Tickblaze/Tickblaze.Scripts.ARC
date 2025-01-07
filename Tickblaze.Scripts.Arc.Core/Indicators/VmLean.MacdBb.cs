using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[AllowNull]
	private Macd _macd;

	[AllowNull]
	private PlotSeries _macdDots;

	[AllowNull]
	private BollingerBands _bollingerBands;

	[NumericRange(MinValue = 1)]
	[Parameter("BB Period", GroupName = "MACDBB Parameters", Description = "Band period for Bollinger Bands")]
	public int BbPeriod { get; set; } = 10;

	// Todo: description.
	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("BB Std. Dev. Multiplier", GroupName = "MACDBB Parameters", Description = "Band std. dev. multiplier for Bollinger Bands")]
	public double BbMultiplier { get; set; } = 1.0;

	[Parameter("BB Channel Color", GroupName = "MACDBB Parameters")]
	public Color BbChannelColor { get; set; } = DrawingColor.DodgerBlue.With(opacity: 0.2f);

	[Plot("BB Average")]
	public PlotSeries BbAverage { get; set; } = new(Color.Transparent, LineStyle.Dot, 2);

	[Plot("BB Upper Band")]
	public PlotSeries BbUpperBand { get; set; } = new(Color.Black, LineStyle.Solid, 2);

	[Plot("BB Lower Band")]
	public PlotSeries BbLowerBand { get; set; } = new(Color.Black, LineStyle.Solid, 2);

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Fast EMA Period", GroupName = "MACDBB Parameters", Description = "Period for fast EMA")]
	public int MacdFastPeriod { get; set; } = 12;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Slow EMA Period", GroupName = "MACDBB Parameters", Description = "Period for slow EMA")]
	public int MacdSlowPeriod { get; set; } = 26;

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
	
	public void InitializeMacdBb()
	{
		_macdDots = new(Color.Transparent);

		_macd = new Macd
		{
			Source = Bars.Close,
			FastPeriod = MacdFastPeriod,
			SlowPeriod = MacdSlowPeriod,
			SignalPeriod = BbPeriod,
		};

		_bollingerBands = new BollingerBands(Bars.Close, BbPeriod, BbMultiplier, MovingAverageType.Simple);

		ShadeBetween(_bollingerBands.Lower, _bollingerBands.Upper, default, BbChannelColor);
	}

	private void CalculateMacdBb(int barIndex)
	{
		if (barIndex is 0)
		{
			return;
		}

		var previousValue = _macdDots[barIndex - 1];
		var currentValue = _macdDots[barIndex] = _macd.Signal[barIndex];
		var upperBandValue = BbUpperBand[barIndex];
		var lowerBandValue = BbLowerBand[barIndex];

		_macdDots.Colors[barIndex] = currentValue.CompareTo(previousValue) switch
		{
			> 0 when currentValue > upperBandValue => MacdRisingAboveChannelDotColor,
			> 0 => MacdRisingBelowChannelDotColor,
			< 0 when currentValue < lowerBandValue => MacdFallingBelowChannelDotColor,
			< 0 => MacdFallingAboveChannelDotColor,
			_ => _macdDots.Colors[barIndex - 1]
		};
	}
}
