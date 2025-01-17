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

	[Parameter("BB Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the Bollinger Bands channel")]
	public Color BbChannelColor { get; set; } = DrawingColor.DodgerBlue.With(opacity: 0.2f);

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Dot Size", GroupName = "MACDBB Visuals", Description = "Size of the MACD dots")]
	public int MacdDotSize { get; set; } = 6;

	[Parameter("MACD Dots Rim Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD dots rim")]
	public Color MacdDotRimColor { get; set; } = Color.Black;

	[Parameter("MACD Rising Dots Above Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an uptrend when they are above the Bollinger Bands channel")]
	public Color MacdRisingAboveChannelDotColor { get; set; } = Color.Green;

	[Parameter("MACD Rising Dots Inside/Below Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an uptrend when they are inside/below the Bollinger Bands channel")]
	public Color MacdRisingBelowChannelDotColor { get; set; } = Color.Green;

	[Parameter("MACD Falling Dots Inside/Above Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an downtrend when they are inside/above the Bollinger Bands channel")]
	public Color MacdFallingAboveChannelDotColor { get; set; } = Color.Red;

	[Parameter("MACD Falling Dots Below Channel Color", GroupName = "MACDBB Visuals", Description = "Color of the MACD points in an downtrend when they are below the Bollinger Bands channel")]
	public Color MacdFallingBelowChannelDotColor { get; set; } = Color.Red;

	[Plot("BB Average")]
	public PlotSeries BbAverage { get; set; } = new(Color.Transparent, LineStyle.Dot, 3);

	[Plot("BB Upper Band")]
	public PlotSeries BbUpperBand { get; set; } = new(Color.Black, LineStyle.Solid, 3);

	[Plot("BB Lower Band")]
	public PlotSeries BbLowerBand { get; set; } = new(Color.Black, LineStyle.Solid, 3);

	[Plot("MACD Connector")]
	public PlotSeries MacdConnector { get; set; } = new(Color.White, LineStyle.Solid, 6);
	
	public void InitializeMacdBb()
	{
		_macdDots = new();

		_macd = new Macd
		{
			Source = Bars.Close,
			FastPeriod = MacdFastPeriod,
			SlowPeriod = MacdSlowPeriod,
			SignalPeriod = BbPeriod,
		};

		_bollingerBands = new BollingerBands(_macd.Signal, BbPeriod, BbMultiplier, MovingAverageType.Simple);

		ShadeBetween(BbLowerBand, BbUpperBand, default, BbChannelColor, BbChannelColor.GetOpacity());
	}

	private void CalculateMacdBb(int barIndex)
	{
		var currentValue = MacdConnector[barIndex] = _macdDots[barIndex] = _macd.Signal[barIndex];
		var previousValue = _macdDots.GetAtOrDefault(barIndex - 1, currentValue);
		var upperBandValue = BbUpperBand[barIndex] = _bollingerBands.Upper[barIndex];
		var lowerBandValue = BbLowerBand[barIndex] = _bollingerBands.Lower[barIndex];

		_macdDots.Colors[barIndex] = currentValue.CompareTo(previousValue) switch
		{
			> 0 when currentValue > upperBandValue => MacdRisingAboveChannelDotColor,
			> 0 => MacdRisingBelowChannelDotColor,
			< 0 when currentValue < lowerBandValue => MacdFallingBelowChannelDotColor,
			< 0 => MacdFallingAboveChannelDotColor,
			_ => _macdDots.Colors.GetAtOrDefault(barIndex - 1, Color.Transparent),
		};
	}

	public void RenderMacdBb(IDrawingContext drawingContext)
	{
		for (var barIndex = Chart.FirstVisibleBarIndex; barIndex <= Chart.LastVisibleBarIndex; barIndex++)
		{
			var point = new Point
			{
				BarIndex = barIndex,
				Price = _macdDots[barIndex],
			};

			var apiPoint = this.GetApiPoint(point);

			var dotRadius = MacdDotSize / 2.0;
			var dotColor = _macdDots.Colors[barIndex];

			drawingContext.DrawEllipse(apiPoint, dotRadius, dotRadius, dotColor, MacdDotRimColor);
		}
	}
}
