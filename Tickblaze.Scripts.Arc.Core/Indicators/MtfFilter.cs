using DynamicData;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: multi panel extension.
public partial class MtfFilter : Indicator
{
	[AllowNull]
    private BarSeries _bars;

    [AllowNull]
	private VmLeanCore _vmLeanCore;

	[AllowNull]
	private MovingAverage _fastMa;

	[AllowNull]
	private MovingAverage _slowMa;

	[AllowNull]
	private Flooding _flooding;

	[NumericRange(MinValue = 1)]
	[Parameter("Bollinger Band Period", Description = "Period of the Bollinger Band")]
	public int BandPeriod { get; set; } = 10;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.5)]
	[Parameter("Bollinger Band Std. Dev. Multiplier", Description = "Std. dev. multiplier of the Bollinger Band")]
	public double BandMultiplier { get; set; } = 1.0;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Fast EMA Period", Description = "Period of the fast MACD EMA")]
	public int MacdFastPeriod { get; set; } = 12;

	[NumericRange(MinValue = 1)]
	[Parameter("MACD Slow EMA Period", Description = "Period of the slow MACD EMA")]
	public int MacdSlowPeriod { get; set; } = 26;

	[NumericRange(MinValue = 1, MaxValue = 200)]
	[Parameter("Swing Strength", Description = "Bar lookback to calculate swing high or low")]
	public int SwingStrength { get; set; } = 3;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Deviation Multiplier", Description = "ATR multipler of the minimum deviation required for a trend change")]
	public double SwingDeviationAtrMultiplier { get; set; }

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Double Top/Bottom ATR Multiplier", Description = "ATR multiplier of the maximum deviation ignored for a double tops or bottoms recognition")]
	public double SwingDtbAtrMultiplier { get; set; }

	[NumericRange(MaxValue = 1)]
	[Parameter("Fast MA Period", Description = "Period of the fast MA")]
	public int FastMaPeriod { get; set; } = 100;

	[Parameter("Fast MA Type", Description = "Smoothing type of the fast MA")]
	public MaType FastMaType { get; set; } = MaType.Exponential;

	[NumericRange(MaxValue = 1)]
	[Parameter("Slow MA Period", Description = "Period of the slow MA")]
	public int SlowMaPeriod { get; set; } = 200;

	[Parameter("Slow MA Type", Description = "Smoothing type of the slow MA")]
	public MaType SlowMaType { get; set; } = MaType.Exponential;

	[Parameter("Histogram Above/Below Zero Line", GroupName = "Requirements", Description = "Whether histogram and zero line comparison is included as a trend")]
	public bool IsHistogramToZeroLineTrendEnabled { get; set; } = true;

	[Parameter("BB + Histogram Above/Below Zero Line", GroupName = "Requirements", Description = "Whether BB + histogram and zero line comparison is included as a trend")]
	public bool IsBbAndHistogramToZeroLineTrendEnabled { get; set; }

	[Parameter("BB Above Upper Band / Below Lower Band", GroupName = "Requirements", Description = "Whether BB and Bollinger Band comparison is included as a trend")]
	public bool IsBbToBollingerBandTrendEnabled { get; set; }

	[Parameter("BB Above/Below Middle Line", GroupName = "Requirements", Description = "Whether BB and middle line comparison is included as a trend")]
	public bool IsBbToMiddleLineTrendEnabled { get; set; }

	[Parameter("BB Above/Below Zero Line", GroupName = "Requirements", Description = "Whether BB and zero line comparison is included as a trend")]
	public bool IsBbToZeroLineTrendEnabled { get; set; }

	[Parameter("Fast MA Above/Below Slow MA", GroupName = "Requirements", Description = "Whether Fast MA and Slow MA comparison is included as a trend")]
	public bool IsFastMaToSlowMaTrendEnabled { get; set; }

	[Parameter("Close Above/Below Fast MA", GroupName = "Requirements", Description = "Whether Close and Fast MA comparison is included as a trend")]
	public bool IsCloseToFastMaTrendEnabled { get; set; }

	[Parameter("Close Above/Below Slow MA", GroupName = "Requirements", Description = "Whether Close and Slow MA comparison is included as a trend")]
	public bool IsCloseToSlowMaTrendEnabled { get; set; } = true;

	[Parameter("Low Above/High Below Fast MA", GroupName = "Requirements", Description = "Whether High/Low and Fast MA comparison is included as a trend")]
	public bool IsHighLowToFastMaTrendEnabled { get; set; }

	[Parameter("Low Above/High Below Slow MA", GroupName = "Requirements", Description = "Whether High/Low and Slow MA comparison is included as a trend")]
	public bool IsHighLowToSlowMaTrendEnabled { get; set; }

	[Parameter("Swing Structure Trend Biases", GroupName = "Requirements", Description = "Whether Swing Structure trend biases are included")]
	public bool IsSwingTrendBiasEnabled { get; set; }

	[Parameter("Bar Basis", GroupName = "Timeframes", Description = "Basis of the indicator bars")]
	public BarType BarTypeValue { get; set; } = BarType.Chart;

	[NumericRange(MinValue = 1)]
	[Parameter("Minute Bar Size", GroupName = "Timeframes", Description = "Size of the minute bar")]
	public int MinuteBarSize { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Range Bar Size", GroupName = "Timeframes", Description = "Size of the range bar")]
	public int RangeBarSize { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Renko Bar Size", GroupName = "Timeframes", Description = "Size of the renko bar")]
	public int RenkoBarSize { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("RenkoBXT Bar Size", GroupName = "Timeframes", Description = "Size of the RenkoBXT bar")]
	public int RenkoBxtBarSize { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("RenkoBXT Offset", GroupName = "Timeframes", Description = "Offset of the RenkoBXT bar")]
	public int RenkoBxtOffset { get; set; } = 4;

	[NumericRange(MinValue = 1)]
	[Parameter("RenkoBXT Reversal", GroupName = "Timeframes", Description = "Reversal of the RenkoBXT bar")]
	public int RenkoBxtReversal { get; set; } = 14;

	[Parameter("Up-trend Color", GroupName = "Visuals", Description = "Color of the up-trend flooding")]
	public Color UpTrendColor { get; set; } = Color.Green.With(opacity: 0.9f);

	[Parameter("Donw-trend Color", GroupName = "Visuals", Description = "Color of the down-trend flooding")]
	public Color DownTrendColor { get; set; } = Color.Red.With(opacity: 0.9f);

	private BarSeries GetBars()
	{
		if (BarTypeValue is BarType.Chart )
		{
			return Bars;
		}

		var barTypeSettings = BarTypeValue switch
		{
			BarType.Minute => new BarPeriod(SourceBarType.Minute, BarPeriod.PeriodType.Minute, MinuteBarSize),
			BarType.Range => new BarPeriod(SourceBarType.Trade, BarPeriod.PeriodType.Range, RangeBarSize),
			BarType.Renko => new BarPeriod(SourceBarType.Trade, BarPeriod.PeriodType.Renko, RenkoBarSize),
			_ => throw new UnreachableException(),
		};

		var barSeriesRequest = new BarSeriesRequest
		{
			SymbolCode = Symbol.Code,
			Period = barTypeSettings,
			Exchange = Symbol.Exchange,
			InstrumentType = Symbol.Type,
			Contract = Bars.ContractSettings,
		};

		return GetBarSeries(barSeriesRequest);
	}

	private static MovingAverageType GetMovingAverageType(MaType maType)
	{
		return maType switch
		{
			MaType.Simple => MovingAverageType.Simple,
			MaType.Exponential => MovingAverageType.Exponential,
			_ => throw new UnreachableException(),
		};
	}

	private ISeries<Trend>[] GetTrendSeriesCollection()
	{
		var barIndexes = _bars.Map((barIndex, bar) => barIndex);

		var trendSelectors = new List<Func<int, Trend>>();

		if (IsHistogramToZeroLineTrendEnabled)
		{
			trendSelectors.Add(GetHistogramToZeroLineTrend);
		}

		if (IsBbAndHistogramToZeroLineTrendEnabled)
		{
			trendSelectors.Add(GetBbAndHistogramToZeroLineTrend);
		}

		if (IsBbToBollingerBandTrendEnabled)
		{
			trendSelectors.Add(GetBbToBollingerBandTrend);
		}

		if (IsBbToMiddleLineTrendEnabled)
		{
			trendSelectors.Add(GetBbToMiddleLineTrend);
		}

		if (IsBbToZeroLineTrendEnabled)
		{
			trendSelectors.Add(GetBbToZeroLineTrend);
		}

		if (IsFastMaToSlowMaTrendEnabled)
		{
			trendSelectors.Add(GetFastMaToSlowMaTrend);
		}

		if (IsCloseToFastMaTrendEnabled)
		{
			trendSelectors.Add(GetCloseToFastMaTrend);
		}

		if (IsCloseToSlowMaTrendEnabled)
		{
			trendSelectors.Add(GetCloseToSlowMaTrend);
		}

		if (IsHighLowToFastMaTrendEnabled)
		{
			trendSelectors.Add(GetHighLowToFastMaTrend);
		}

		if (IsHighLowToSlowMaTrendEnabled)
		{
			trendSelectors.Add(GetHighLowToSlowMaTrend);
		}

		if (IsSwingTrendBiasEnabled)
		{
			trendSelectors.Add(GetSwingTrendBias);
		}

		return trendSelectors
			.Select(selector => barIndexes.Map(selector))
			.ToArray();
	}

	protected override Parameters GetParameters(Parameters parameters)
    {
		List<string> propertyNames =
		[
			nameof(MinuteBarSize),
			nameof(RangeBarSize),
			nameof(RenkoBarSize),
			nameof(RenkoBarSize),
			nameof(RenkoBxtOffset),
			nameof(RenkoBxtReversal),
		];

		_ = BarTypeValue switch
		{
			BarType.Minute => propertyNames.Remove(nameof(MinuteBarSize)),
			BarType.Range => propertyNames.Remove(nameof(RangeBarSize)),
			BarType.Renko => propertyNames.Remove(nameof(RenkoBarSize)),
			BarType.RenkoBxt => propertyNames.Remove(nameof(RenkoBxtBarSize))
				& propertyNames.Remove(nameof(RenkoBxtOffset))
				& propertyNames.Remove(nameof(RenkoBxtReversal)),
			_ => false,
		};

		parameters.RemoveRange(propertyNames);

		return parameters;
	}
    
	protected override void Initialize()
    {
		_bars = GetBars();

		_vmLeanCore = new VmLeanCore
		{
			Bars = _bars,
			RenderTarget = this,

			BandPeriod = BandPeriod,
			BandMultiplier = BandMultiplier,
			MacdSlowPeriod = MacdSlowPeriod,
			MacdFastPeriod = MacdFastPeriod,

			SwingStrength = SwingStrength,
			SwingDtbAtrMultiplier = SwingDtbAtrMultiplier,
			SwingDeviationAtrMultiplier = SwingDeviationAtrMultiplier,
		};

		_fastMa = new MovingAverage
		{
			Bars = _bars,
			Source = _bars.Close,
			Period = FastMaPeriod,
			Type = GetMovingAverageType(FastMaType),
		};

		_slowMa = new MovingAverage
		{
			Bars = _bars,
			Source = _bars.Close,
			Period = SlowMaPeriod,
			Type = GetMovingAverageType(SlowMaType),
		};

		var trendSeriesCollection = GetTrendSeriesCollection();

		_flooding = new Flooding
		{
			RenderTarget = this,
			UpTrendColor = UpTrendColor,
			DownTrendColor = DownTrendColor,
			TrendSeriesCollection = trendSeriesCollection,
		};
	}

	private Trend GetHistogramToZeroLineTrend(int barIndex)
	{
		var histogramValue = _vmLeanCore.Histogram[barIndex];

		return histogramValue.EpsilonCompare(0).ToTrend();
	}

	private Trend GetBbAndHistogramToZeroLineTrend(int barIndex)
	{
		var bbValue = _vmLeanCore.Macd.Signal[barIndex];
		var histogramValue = _vmLeanCore.Histogram[barIndex];

		if (bbValue.EpsilonGreaterThan(0) && histogramValue.EpsilonGreaterThan(0))
		{
			return Trend.Up;
		}
		else if (bbValue.EpsilonLessThan(0) && histogramValue.EpsilonLessThan(0))
		{
			return Trend.Down;
		}

		return Trend.None;
	}

	private Trend GetBbToBollingerBandTrend(int barIndex)
	{
		var bbValue = _vmLeanCore.Macd.Signal[barIndex];
		var upperBandValue = _vmLeanCore.BollingerBands.Upper[barIndex];
		var lowerBandValue = _vmLeanCore.BollingerBands.Lower[barIndex];

		if (bbValue.EpsilonGreaterThan(upperBandValue))
		{
			return Trend.Up;
		}
		else if (bbValue.EpsilonLessThan(lowerBandValue))
		{
			return Trend.Down;
		}

		return Trend.None;
	}

	private Trend GetBbToMiddleLineTrend(int barIndex)
	{
		var bbValue = _vmLeanCore.Macd.Signal[barIndex];
		var upperBandValue = _vmLeanCore.BollingerBands.Upper[barIndex];
		var lowerBandValue = _vmLeanCore.BollingerBands.Lower[barIndex];
		var middleValue = (upperBandValue + lowerBandValue) / 2.0;

		return bbValue.EpsilonCompare(middleValue).ToTrend();
	}

	private Trend GetBbToZeroLineTrend(int barIndex)
	{
		var bbValue = _vmLeanCore.Macd.Signal[barIndex];

		return bbValue.EpsilonCompare(0).ToTrend();
	}

	private Trend GetFastMaToSlowMaTrend(int barIndex)
	{
		var fastMaValue = _fastMa[barIndex];
		var slowMaValue = _slowMa[barIndex];

		return fastMaValue.EpsilonCompare(slowMaValue).ToTrend();
	}

	private Trend GetCloseToFastMaTrend(int barIndex)
	{
		var fastMaValue = _fastMa[barIndex];
		var closeValue = _bars.Close[barIndex];

		return closeValue.EpsilonCompare(fastMaValue).ToTrend();
	}

	private Trend GetCloseToSlowMaTrend(int barIndex)
	{
		var slowMaValue = _slowMa[barIndex];
		var closeValue = _bars.Close[barIndex];

		return closeValue.EpsilonCompare(slowMaValue).ToTrend();
	}

	private Trend GetHighLowToFastMaTrend(int barIndex)
	{
		var lowValue = _bars.Low[barIndex];
		var highValue = _bars.High[barIndex];
		var fastMaValue = _fastMa[barIndex];

		if (lowValue.EpsilonGreaterThan(fastMaValue))
		{
			return Trend.Up;
		}
		else if (highValue.EpsilonLessThan(fastMaValue))
		{
			return Trend.Down;
		}

		return Trend.None;
	}

	private Trend GetHighLowToSlowMaTrend(int barIndex)
	{
		var lowValue = _bars.Low[barIndex];
		var highValue = _bars.High[barIndex];
		var slowMaValue = _slowMa[barIndex];

		if (lowValue.EpsilonGreaterThan(slowMaValue))
		{
			return Trend.Up;
		}
		else if (highValue.EpsilonLessThan(slowMaValue))
		{
			return Trend.Down;
		}

		return Trend.None;
	}

	private Trend GetSwingTrendBias(int barIndex)
	{
		return _vmLeanCore.Swings.TrendBiases[barIndex];
	}

	public override void OnRender(IDrawingContext drawingContext)
    {
		_flooding.OnRender(drawingContext);
	}

    public enum BarType
	{
		[DisplayName("Chart")]
		Chart,

		[DisplayName("Minute")]
		Minute,

		[DisplayName("Range")]
		Range,

		[DisplayName("Renko")]
		Renko,

		[DisplayName("RenkoBXT")]
		RenkoBxt,
	}

	public enum MaType
	{
		[DisplayName("SMA")]
		Simple,

		[DisplayName("EMA")]
		Exponential,
	}
}
