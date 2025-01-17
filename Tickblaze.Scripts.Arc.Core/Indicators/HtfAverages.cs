using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: specify the same contract type as source symbol.
public partial class HtfAverages : Indicator
{
	public HtfAverages()
	{
		IsOverlay = true;
		
		ShortName = "HA";
		
		Name = "HTF Averages";
	}

	private int _maMaxPeriod;

	private const int _maCount = 7;

	private const string _autoLabel = "<auto>";

	[AllowNull]
	private int[] _maPeriods;

	[AllowNull]
	private PlotSeries[] _maPlots;

	[AllowNull]
	private Indicator?[] _maIndicators;

	[AllowNull]
	private BarSeries _higherTimeframeBars;

	[Parameter("MA Timeframe")]
	public Timeframe TimeframeValue { get; set; } = Timeframe.Day;

	[NumericRange(MinValue = 1)]
	[Parameter("MA Time Value")]
	public int TimeInMinutes { get; set; } = 240;

	[Parameter("MA Type")]
	public MovingAverageType MovingAverageTypeValue { get; set; } = MovingAverageType.Exponential;
	
	[Parameter("Show MAs Continuously")]
	public bool ShowMasContinuously { get; set; }

	[NumericRange]
	[Parameter("MA Period 1")]
	public int MaPeriod1 { get; set; } = 6;

	[NumericRange]
	[Parameter("MA Period 2")]
	public int MaPeriod2 { get; set; } = 12;

	[NumericRange]
	[Parameter("MA Period 3")]
	public int MaPeriod3 { get; set; } = 25;

	[NumericRange]
	[Parameter("MA Period 4")]
	public int MaPeriod4 { get; set; } = 50;

	[NumericRange]
	[Parameter("MA Period 5")]
	public int MaPeriod5 { get; set; } = 100;

	[NumericRange]
	[Parameter("MA Period 6")]
	public int MaPeriod6 { get; set; }

	[NumericRange]
	[Parameter("MA Period 7")]
	public int MaPeriod7 { get; set; }

	[Parameter("Show Labels", GroupName = "Visuals")]
	public bool ShowLabels { get; set; } = true;

	[Parameter("Label Font", GroupName = "Visuals")]
	public Font LabelFont { get; set; } = new Font("Arial", 12);

	[NumericRange(MinValue = 1)]
	[Parameter("Max Line Length in Pixels", GroupName = "Visuals")]
	public int MaxLineLengthInPixels { get; set; } = 100;

	[Parameter("Line Origin", GroupName = "Visuals")]
	public LineOrigin LineOriginValue { get; set; } = LineOrigin.FromLeft;

	[Parameter("MA Label 1", GroupName = "Visuals")]
	public string MaLabel1 { get; set; } = _autoLabel;

	[Parameter("MA Label 2", GroupName = "Visuals")]
	public string MaLabel2 { get; set; } = _autoLabel;

	[Parameter("MA Label 3", GroupName = "Visuals")]
	public string MaLabel3 { get; set; } = _autoLabel;

	[Parameter("MA Label 4", GroupName = "Visuals")]
	public string MaLabel4 { get; set; } = _autoLabel;

	[Parameter("MA Label 5", GroupName = "Visuals")]
	public string MaLabel5 { get; set; } = _autoLabel;

	[Parameter("MA Label 6", GroupName = "Visuals")]
	public string MaLabel6 { get; set; } = _autoLabel;

	[Parameter("MA Label 7", GroupName = "Visuals")]
	public string MaLabel7 { get; set; } = _autoLabel;

	[Plot("Plot1")]
	public PlotSeries MaPlot1 { get; set; } = new(DrawingColor.Goldenrod);

	[Plot("Plot2")]
	public PlotSeries MaPlot2 { get; set; } = new(Color.White);

	[Plot("Plot3")]
	public PlotSeries MaPlot3 { get; set; } = new(Color.Yellow);

	[Plot("Plot4")]
	public PlotSeries MaPlot4 { get; set; } = new(Color.Black);

	[Plot("Plot5")]
	public PlotSeries MaPlot5 { get; set; } = new(Color.Red);

	[Plot("Plot6")]
	public PlotSeries MaPlot6 { get; set; } = new(Color.Cyan);

	[Plot("Plot7")]
	public PlotSeries MaPlot7 { get; set; } = new(DrawingColor.Lime);

	private BarPeriod GetBarType()
	{
		return TimeframeValue switch
		{
			Timeframe.Day => new BarPeriod(SourceBarType.Day, BarType.Day, 1),
			Timeframe.Minute => new BarPeriod(SourceBarType.Minute, BarType.Minute, TimeInMinutes),
			_ => throw new UnreachableException()
		};
	}

	private Indicator GetMovingAverageSeries(BarSeries barSeries, int period)
	{
		return MovingAverageTypeValue switch
		{
			MovingAverageType.Simple => new SimpleMovingAverage
			{
				Period = period,
				Bars = barSeries,
				Source = barSeries.Close,
			},
			MovingAverageType.Exponential => new ExponentialMovingAverage
			{
				Period = period,
				Bars = barSeries,
				Source = barSeries.Close,
			},
			_ => throw new UnreachableException(),
		};
	}

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
		_maIndicators = new Indicator?[_maCount];

		_maPeriods =
		[
			MaPeriod1,
			MaPeriod2,
			MaPeriod3,
			MaPeriod4,
			MaPeriod5,
			MaPeriod6,
			MaPeriod7,
		];

		_maPlots =
		[
			MaPlot1,
			MaPlot2,
			MaPlot3,
			MaPlot4,
			MaPlot5,
			MaPlot6,
			MaPlot7,
		];
		
		_maMaxPeriod = _maPeriods.Max();

		var barSeriesRequest = new BarSeriesRequest
		{
			Period = GetBarType(),
			SymbolCode = Symbol.Code,
			Exchange = Symbol.Exchange,
			InstrumentType = Symbol.Type,
			Contract = Bars.ContractSettings,
		};

		_higherTimeframeBars = GetBarSeries(barSeriesRequest);

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPeriod = _maPeriods[maIndex];

			_maIndicators[maIndex] = null;

			if (maPeriod is not 0)
			{
				_maIndicators[maIndex] = GetMovingAverageSeries(_higherTimeframeBars, maPeriod);
			}
		}
	}

	protected override void Calculate(int barIndex)
	{
		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPlot = _maPlots[maIndex];
			var ma = _maIndicators[maIndex];

			if (ma is null || _higherTimeframeBars.Count is 0)
			{
				continue;
			}

			ma.Calculate();

			if (ShowMasContinuously && ma.Plots is [var plot])
			{
				// Todo: backfill [do prelast].
				maPlot[barIndex] = plot.Last();
			}
		}
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var visibleEndBarIndex = Chart.LastVisibleBarIndex;
			var visibleEndTimeUtc = Bars.Time[visibleEndBarIndex];
			var visibleStartBarIndex = Chart.FirstVisibleBarIndex;
			var visibleStartTimeUtc = Bars.Time[visibleStartBarIndex];
			
			var exchangeCalendar = Symbol.ExchangeCalendar;
			var visibleEndSession = exchangeCalendar.GetSession(visibleEndTimeUtc);

			var maPlot = _maPlots[maIndex];
			var maIndicator = _maIndicators[maIndex];
			var maEndBarIndex = _higherTimeframeBars.GetBarIndex(visibleEndTimeUtc);
			var maStartBarIndex = _higherTimeframeBars.GetBarIndex(visibleStartTimeUtc);

			if (maEndBarIndex is -1
				|| !maPlot.IsVisible
				|| maStartBarIndex is -1
				|| visibleEndSession is null
				|| maIndicator?.Plots is not [var maResult])
			{
				continue;
			}

			var maValue = maResult[maEndBarIndex];
			var maValueText = _maPeriods[maIndex].ToString();
			var maValueY = ChartScale.GetYCoordinateByValue(maValue);

			if (TimeframeValue is Timeframe.Day)
			{
				maValueText += "-D";
			}

			var maValueTextSize = drawingContext.MeasureText(maValueText, LabelFont);
			
			var visibleEndX = Chart.GetXCoordinateByTime(visibleEndTimeUtc);
			var visibleStartX = Chart.GetXCoordinateByTime(visibleStartTimeUtc);
			var visibleEndSessionStartX = Chart.GetXCoordinateByTime(visibleEndSession.StartUtcDateTime);
			
			double endLineX;
			double startLineX;
			double textStartX;

			if (LineOriginValue is LineOrigin.FromLeft)
			{
				startLineX = Math.Max(visibleStartX, visibleEndSessionStartX);

				endLineX = Math.Min(startLineX + MaxLineLengthInPixels, visibleEndX);

				textStartX = ShowMasContinuously ? visibleStartX : startLineX;
			}
			else if (LineOriginValue is LineOrigin.FromRight)
			{
				endLineX = visibleEndX;

				textStartX = visibleEndX - maValueTextSize.Width;

				startLineX = Math.Max(endLineX - MaxLineLengthInPixels, visibleEndSessionStartX);
			}
			else
			{
				throw new UnreachableException();
			}

			if (!ShowMasContinuously)
			{
				drawingContext.DrawHorizontalLine(startLineX, maValueY, endLineX, maPlot.Color);
			}

			drawingContext.DrawText(textStartX, maValueY + VerticalMargin, maValueText, maPlot.Color, LabelFont);
		}
	}
}