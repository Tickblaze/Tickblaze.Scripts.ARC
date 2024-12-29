using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public partial class HtfAverages : Indicator
{
	public HtfAverages()
	{
		IsOverlay = true;
		ShortName = "HTFMA";
		Name = "ARC HTF Averages";
	}

	private const int _maCount = 7;
	private const int _barLookbackCount = 256;
	private const string _autoLabel = "<auto>";

	private BarSeries _higherTimeframeBars = default!;
	private Indicator?[] _maIndicators = new Indicator?[_maCount];

	[Parameter("Bkg Timeframe")]
	public Timeframe TimeframeValue { get; set; } = Timeframe.Day;

	[NumericRange(MinValue = 1)]
	[Parameter("Bkg Time Value")]
	public int TimeInMinutes { get; set; } = 240;

	[Parameter("MA Type")]
	public MovingAverageType MovingAverageTypeValue { get; set; } = MovingAverageType.Exponential;

	[Parameter("Show Label?", GroupName = "Lines & Labels")]
	public bool ShowLabels { get; set; } = true;

	[Parameter("Font", GroupName = "Lines & Labels")]
	public Font Font { get; set; } = new Font("Arial", 12);

	[Parameter("Show MAs continuously?", GroupName = "Lines & Labels")]
	public bool IsRestrictedToLastDay { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Max Line Length (pixels)", GroupName = "Lines & Labels")]
	public int MaxLineLengthInPixels { get; set; } = 100;

	[Parameter("Line Origin", GroupName = "Lines & Labels")]
	public LineOrigin LineOriginValue { get; set; } = LineOrigin.FromLeft;

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 1")]
	public int MaPeriod1 { get; set; } = 6;

	[Parameter("MA Label", GroupName = "MA 1")]
	public string MaLabel1 { get; set; } = _autoLabel;

	[Plot("Plot1")]
	public PlotSeries MaPlot1 { get; set; } = new(Color.FromDrawingColor(DrawingColor.Goldenrod));

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 2")]
	public int MaPeriod2 { get; set; } = 12;

	[Parameter("MA Label", GroupName = "MA 2")]
	public string MaLabel2 { get; set; } = _autoLabel;

	[Plot("Plot2")]
	public PlotSeries MaPlot2 { get; set; } = new(Color.White);

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 3")]
	public int MaPeriod3 { get; set; } = 25;

	[Parameter("MA Label", GroupName = "MA 3")]
	public string MaLabel3 { get; set; } = _autoLabel;

	[Plot("Plot3")]
	public PlotSeries MaPlot3 { get; set; } = new(Color.Yellow);

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 4")]
	public int MaPeriod4 { get; set; } = 50;

	[Parameter("MA Label", GroupName = "MA 4")]
	public string MaLabel4 { get; set; } = _autoLabel;

	[Plot("Plot4")]
	public PlotSeries MaPlot4 { get; set; } = new(Color.Black);

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 5")]
	public int MaPeriod5 { get; set; } = 100;

	[Parameter("MA Label", GroupName = "MA 5")]
	public string MaLabel5 { get; set; } = _autoLabel;

	[Plot("Plot5")]
	public PlotSeries MaPlot5 { get; set; } = new(Color.Red);

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 6")]
	public int MaPeriod6 { get; set; }

	[Parameter("MA Label", GroupName = "MA 6")]
	public string MaLabel6 { get; set; } = _autoLabel;

	[Plot("Plot6")]
	public PlotSeries MaPlot6 { get; set; } = new(Color.Cyan);

	[NumericRange]
	[Parameter("MA Period", GroupName = "MA 7")]
	public int MaPeriod7 { get; set; }

	[Parameter("MA Label", GroupName = "MA 7")]
	public string MaLabel7 { get; set; } = _autoLabel;

	[Plot("Plot7")]
	public PlotSeries MaPlot7 { get; set; } = new(Color.FromDrawingColor(DrawingColor.Lime));

	// Todo: add lookback days.

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (TimeframeValue is Timeframe.Day)
		{
			parameters.Remove(nameof(TimeInMinutes));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		int[] maPeriods =
		[
			MaPeriod1,
			MaPeriod2,
			MaPeriod3,
			MaPeriod4,
			MaPeriod5,
			MaPeriod6,
			MaPeriod7,
		];

		var barSeriesRequest = new BarSeriesRequest
		{
			// What to do with series contract?
			SymbolCode = Symbol.Code,
			Exchange = Symbol.Exchange,
			InstrumentType = Symbol.Type,
			Period = GetBarType(TimeframeValue, TimeInMinutes),
			StartTimeUtc = GetStartTimeUtc(TimeframeValue, TimeInMinutes),
		};

		_higherTimeframeBars = GetBarSeries(barSeriesRequest);

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPeriod = maPeriods[maIndex];

			_maIndicators[maIndex] = null;

			if (maPeriod is not 0)
			{
				_maIndicators[maIndex] = GetMovingAverageSeries(_higherTimeframeBars, maPeriod, MovingAverageTypeValue);
			}
		}
	}

	protected override void Calculate(int index)
	{
		PlotSeries[] maPlots =
		[
			MaPlot1,
			MaPlot2,
			MaPlot3,
			MaPlot4, 
			MaPlot5,
			MaPlot6,
			MaPlot7,
		];

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPlot = maPlots[maIndex];
			var maIndicator = _maIndicators[maIndex];

			if (maIndicator is null || _higherTimeframeBars.Count is 0)
			{
				continue;
			}

			maIndicator.Calculate();

			maPlot[index] = maIndicator.Plots is not [var plot] ? double.NaN : plot.Last();
		}
	}

	private static BarPeriod GetBarType(Timeframe timeframe, int timeInMinutes)
	{
		return timeframe switch
		{
			Timeframe.Day => new BarPeriod(BarPeriod.SourceType.Day, BarPeriod.PeriodType.Day, 1),
			Timeframe.Minute => new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, timeInMinutes),
			_ => throw new UnreachableException()
		};
	}

	private DateTime? GetStartTimeUtc(Timeframe timeframe, int timeInMinutes)
	{
		var utcNow = DateTime.UtcNow;

		return timeframe switch
		{
			Timeframe.Day => utcNow.AddDays(-_barLookbackCount),
			Timeframe.Minute => utcNow.AddMinutes(-timeInMinutes * _barLookbackCount),
			_ => throw new UnreachableException()
		};
	}

	private static Indicator GetMovingAverageSeries(BarSeries barSeries, int period, MovingAverageType movingAverageType)
	{
		return movingAverageType switch
		{
			MovingAverageType.Simple => new SimpleMovingAverage(barSeries, barSeries.Close, period),
			MovingAverageType.Exponential => new ExponentialMovingAverage(barSeries, barSeries.Close, period),
			_ => throw new UnreachableException(),
		};
	}
}
