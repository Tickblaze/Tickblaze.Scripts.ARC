using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;
using HtfMa = Tickblaze.Scripts.Arc.Common.DrawingPartDictionary<int, Tickblaze.Scripts.Arc.Common.PriceInterval>;

namespace Tickblaze.Scripts.Arc.Core;

public partial class HtfAverages : Indicator
{
	public HtfAverages()
	{
		IsOverlay = true;
		
		ShortName = "HA";
		
		Name = "HTF Averages";
	}

	private int _currentBarIndex;

	private const int _maCount = 7;

	private const string _autoMaLabel = "<auto>";

	[AllowNull]
	private int[] _maPeriods;

	[AllowNull]
	private string[] _maLabels;

	[AllowNull]
	private PlotSeries[] _maPlots;

	[AllowNull]
	private Indicator?[] _maIndicators;

	[AllowNull]
	private HtfMa[] _maIntervalsCollection;
	
	[AllowNull]
	private BarSeries _higherTimeframeBars;

	[Parameter("MA Timeframe", Description = "Timeframe of the MAs")]
	public Timeframe TimeframeValue { get; set; } = Timeframe.Day;

	[NumericRange(MinValue = 1)]
	[Parameter("MA Timeframe Bar Size", Description = "Bar size of the minute MAs")]
	public int MinuteBarSize { get; set; } = 240;

	[Parameter("MA Type", Description = "Type of the MAs")]
	public MaType MaTypeValue { get; set; } = MaType.Exponential;
	
	[Parameter("Show MAs Continuously", Description = "Whether MAs are shown with lines")]
	public bool ShowMasContinuously { get; set; }

	[NumericRange]
	[Parameter("MA Period 1", Description = "Period of the MA 1")]
	public int MaPeriod1 { get; set; } = 6;

	[NumericRange]
	[Parameter("MA Period 2", Description = "Period of the MA 2")]
	public int MaPeriod2 { get; set; } = 12;

	[NumericRange]
	[Parameter("MA Period 3", Description = "Period of the MA 3")]
	public int MaPeriod3 { get; set; } = 25;

	[NumericRange]
	[Parameter("MA Period 4", Description = "Period of the MA 4")]
	public int MaPeriod4 { get; set; } = 50;

	[NumericRange]
	[Parameter("MA Period 5", Description = "Period of the MA 5")]
	public int MaPeriod5 { get; set; } = 100;

	[NumericRange]
	[Parameter("MA Period 6", Description = "Period of the MA 6")]
	public int MaPeriod6 { get; set; }

	[NumericRange]
	[Parameter("MA Period 7", Description = "Period of the MA 7")]
	public int MaPeriod7 { get; set; }

	[Parameter("Show Labels", GroupName = "Visuals", Description = "Whether MA labels are shown")]
	public bool ShowLabels { get; set; } = true;

	[Parameter("Line Origin", GroupName = "Visuals", Description = "Aligment of the MA lines and labels")]
	public Aligment AligmentValue { get; set; } = Aligment.Left;

	[Parameter("Label Font", GroupName = "Visuals", Description = "Font of the MA labels")]
	public Font LabelFont { get; set; } = new("Arial", 12);

	[NumericRange(MinValue = 1)]
	[Parameter("Max Line Length in Pixels", GroupName = "Visuals", Description = "Maximum length of the line in pixels")]
	public int MaxLineLengthInPixels { get; set; } = 100;

	[Parameter("MA Label 1", GroupName = "Visuals", Description = "Label of the MA 1")]
	public string MaLabel1 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 2", GroupName = "Visuals", Description = "Label of the MA 2")]
	public string MaLabel2 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 3", GroupName = "Visuals", Description = "Label of the MA 3")]
	public string MaLabel3 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 4", GroupName = "Visuals", Description = "Label of the MA 4")]
	public string MaLabel4 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 5", GroupName = "Visuals", Description = "Label of the MA 5")]
	public string MaLabel5 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 6", GroupName = "Visuals", Description = "Label of the MA 6")]
	public string MaLabel6 { get; set; } = _autoMaLabel;

	[Parameter("MA Label 7", GroupName = "Visuals", Description = "Label of the MA 7")]
	public string MaLabel7 { get; set; } = _autoMaLabel;

	[Plot("MA 1")]
	public PlotSeries MaPlot1 { get; set; } = new(DrawingColor.Goldenrod);

	[Plot("MA 2")]
	public PlotSeries MaPlot2 { get; set; } = new(Color.White);

	[Plot("MA 3")]
	public PlotSeries MaPlot3 { get; set; } = new(Color.Yellow);

	[Plot("MA 4")]
	public PlotSeries MaPlot4 { get; set; } = new(Color.Black);

	[Plot("MA 5")]
	public PlotSeries MaPlot5 { get; set; } = new(Color.Red);

	[Plot("MA 6")]
	public PlotSeries MaPlot6 { get; set; } = new(Color.Cyan);

	[Plot("MA 7")]
	public PlotSeries MaPlot7 { get; set; } = new(DrawingColor.Lime);

	private DateTime GetStartTimeUtc()
	{
		var maMaxPeriod = _maPeriods.Max() + 1;

		var deltaTimeSpan = TimeframeValue switch
		{
			Timeframe.Day => TimeSpan.FromDays(1),
			Timeframe.Minute => TimeSpan.FromMinutes(MinuteBarSize),
			_ => throw new UnreachableException(),
		};

		var startTimeUtc = DateTime.UtcNow - maMaxPeriod * deltaTimeSpan;

		return startTimeUtc;
	}

	private string GetMaLabel(int maIndex)
	{
		var maLabel = _maLabels[maIndex];
		var maPeriod = _maPeriods[maIndex];

		var maLabelSuffix = TimeframeValue switch
		{
			_ when !string.Equals(maLabel, _autoMaLabel, StringComparison.Ordinal) => "-" + maLabel,
			Timeframe.Day => "-D",
			_ => "",
		};

		return maPeriod + maLabelSuffix;
	}

	private BarPeriod GetBarType()
	{
		return TimeframeValue switch
		{
			Timeframe.Day => new BarPeriod(SourceBarType.Day, BarType.Day, 1),
			Timeframe.Minute => new BarPeriod(SourceBarType.Minute, BarType.Minute, MinuteBarSize),
			_ => throw new UnreachableException()
		};
	}

	private MovingAverageType GetMovingAverageType()
	{
		return MaTypeValue switch
		{
			MaType.Simple => MovingAverageType.Simple,
			MaType.Exponential => MovingAverageType.Exponential,
			_ => throw new UnreachableException(),
		};
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (TimeframeValue is Timeframe.Day)
		{
			parameters.Remove(nameof(MinuteBarSize));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_currentBarIndex = 0;

		_maIntervalsCollection = new HtfMa[_maCount];
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

		_maLabels =
		[
			MaLabel1,
			MaLabel2,
			MaLabel3,
			MaLabel4,
			MaLabel5,
			MaLabel6,
			MaLabel7,
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

		var barType = GetBarType();
		var maType = GetMovingAverageType();
		var startTimeUtc = GetStartTimeUtc();

		var barSeriesRequest = new BarSeriesRequest
		{
			Period = barType,
			SymbolCode = Symbol.Code,
			Exchange = Symbol.Exchange,
			//StartTimeUtc = startTimeUtc,
			InstrumentType = Symbol.Type,
			Contract = Bars.ContractSettings,
		};

		_higherTimeframeBars = GetBars(barSeriesRequest);

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPeriod = _maPeriods[maIndex];

			_maIntervalsCollection[maIndex] = [];

			_maIndicators[maIndex] = null;

			if (maPeriod is not 0)
			{
				_maIndicators[maIndex] = new MovingAverage
				{
					Type = maType,
					Period = maPeriod,
					Bars = _higherTimeframeBars,
					Source = _higherTimeframeBars.Close,
				};
			}
		}
	}

    protected override void Calculate(int barIndex)
	{
		var lastHtfBarIndex = _higherTimeframeBars.Count - 1;
		
		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maIndicator = _maIndicators[maIndex];
			var htfMa = _maIntervalsCollection[maIndex];

			if (maIndicator is null)
			{
				continue;
			}

			maIndicator.Calculate();

			var isNewBar = !_currentBarIndex.Equals(lastHtfBarIndex);

			var lastMaValue = maIndicator.Plots is [var maResult]
				? maResult.GetLastOrDefault(double.NaN) : double.NaN;

			if (!htfMa.IsEmpty)
			{
				var lastInterval = htfMa.LastDrawingPart;
				
				lastInterval.Price = lastMaValue;
				
				if (!isNewBar)
				{
					lastInterval.EndBarIndex = barIndex;
				}
			}

			if (isNewBar || barIndex is 0 && htfMa.IsEmpty)
			{
				var interval = new PriceInterval
				{
					Price = lastMaValue,
					EndBarIndex = barIndex,
					StartBarIndex = barIndex,
				};

				htfMa.AddOrUpdate(interval);
			}
		}

		_currentBarIndex = lastHtfBarIndex;
	}

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (ShowMasContinuously)
		{
			RenderContinuousMas(drawingContext);
		}
		else
		{
			RenderDiscontinuousMas(drawingContext);
		}
    }

	private void RenderContinuousMas(IDrawingContext drawingContext)
	{
		var visibleBoundary = this.GetVisibleBoundary();

        for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPlot = _maPlots[maIndex];
            var maIntervalsCollection = _maIntervalsCollection[maIndex];

			var maIntervals = maIntervalsCollection.GetVisibleDrawingParts(visibleBoundary)
				.OrderBy(interval => interval.StartBarIndex)
				.ToArray();

			if (maIntervals.Length is 0 || !maPlot.IsVisible)
			{
				continue;
			}

			ApiPoint maLabelApiPoint;

			var maLabelText = GetMaLabel(maIndex);
			var maLastInterval = maIntervals.GetLast();
			var maFirstInterval = maIntervals.GetAt(0);
			
			if (AligmentValue is Aligment.Left)
			{
				var maLabelX = Chart.GetLeftX();
				var maLabelPrice = maFirstInterval.Price;

				maLabelApiPoint = new ApiPoint
				{
					X = maLabelX + HorizontalMargin,
					Y = ChartScale.GetYCoordinateByValue(maLabelPrice) + VerticalMargin,
				};
			}
			else
			{
				var maLabelPrice = maLastInterval.Price;
				var maLabelTextSize = drawingContext.MeasureText(maLabelText, LabelFont);

				var lastBarIndex = Math.Min(maLastInterval.EndBarIndex, Chart.LastVisibleBarIndex);
				
				maLabelApiPoint = new ApiPoint
				{
					X = Chart.GetXCoordinateByBarIndex(lastBarIndex) - maLabelTextSize.Width - HorizontalMargin,
					Y = ChartScale.GetYCoordinateByValue(maLabelPrice) + VerticalMargin,
				};
			}

			var apiPoints = maIntervals
				.SelectMany<PriceInterval, Point>(interval => [interval.StartPoint, interval.EndPoint])
				.Select(this.GetApiPoint);

			drawingContext.DrawPolygon(apiPoints, default, maPlot.Color, maPlot.Thickness, maPlot.LineStyle);

			if (ShowLabels)
			{
				drawingContext.DrawText(maLabelApiPoint, maLabelText, maPlot.Color, LabelFont);
			}
		}
	}

	private void RenderDiscontinuousMas(IDrawingContext drawingContext)
    {
		var visibleBoundary = this.GetVisibleBoundary();

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
			var maIntervalsCollection = _maIntervalsCollection[maIndex];
			var maLastInterval = maIntervalsCollection.GetVisibleDrawingParts(visibleBoundary)
				.OrderByDescending(interval => interval.StartBarIndex)
				.FirstOrDefault();

			if (!maPlot.IsVisible
                || maLastInterval is null
				|| visibleEndSession is null
                || maIndicator?.Plots is not [var maResult])
            {
                continue;
            }

            var maValue = maLastInterval.Price;
            var maValueText = GetMaLabel(maIndex);
            var maValueY = ChartScale.GetYCoordinateByValue(maValue);
            var maValueTextSize = drawingContext.MeasureText(maValueText, LabelFont);

            var visibleStartX = Chart.GetLeftX();
            var visibleEndX = Chart.GetXCoordinateByTime(visibleEndTimeUtc);
            var visibleEndSessionStartX = Chart.GetXCoordinateByTime(visibleEndSession.StartUtcDateTime);

            double endLineX;
            double startLineX;
            double textStartX;

            if (AligmentValue is Aligment.Left)
            {
                startLineX = Math.Max(visibleStartX, visibleEndSessionStartX);

                endLineX = Math.Min(startLineX + MaxLineLengthInPixels, visibleEndX);

                textStartX = startLineX + HorizontalMargin;
            }
            else
            {
                endLineX = visibleEndX;

                textStartX = visibleEndX - maValueTextSize.Width - HorizontalMargin;

                startLineX = Math.Max(endLineX - MaxLineLengthInPixels, visibleEndSessionStartX);
            }

            drawingContext.DrawHorizontalLine(startLineX, maValueY, endLineX, maPlot.Color);

			if (ShowLabels)
			{
				drawingContext.DrawText(textStartX, maValueY + VerticalMargin, maValueText, maPlot.Color, LabelFont);
			}
        }
    }

    public enum Aligment
	{
		[DisplayName("Left")]
		Left,

		[DisplayName("Right")]
		Right,
	}

	public enum MaType
	{
		[DisplayName("SMA")]
		Simple,

		[DisplayName("EMA")]
		Exponential,
	}

	public enum Timeframe
	{
		[DisplayName("Day")]
		Day,

		[DisplayName("Minutes")]
		Minute,
	}
}