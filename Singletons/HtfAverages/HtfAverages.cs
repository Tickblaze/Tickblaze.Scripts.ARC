using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Community;

public partial class HtfAverages : Indicator
{
	public HtfAverages()
	{
		IsOverlay = true;
		
		ShortName = "HA";
		
		Name = "HTF Averages";

		MaPlot1 = new("MA 1", DrawingColor.Goldenrod, this);
		MaPlot2 = new("MA 2", Color.White, this);
		MaPlot3 = new("MA 3", Color.Yellow, this);
		MaPlot4 = new("MA 4", Color.Black, this);
		MaPlot5 = new("MA 5", Color.Red, this);
		MaPlot6 = new("MA 6", Color.Cyan, this);
		MaPlot7 = new("MA 7", DrawingColor.Lime, this);
	}

	private int _currentHtfBarIndex;

	private const int _maCount = 7;

	private const string _autoMaLabel = "<auto>";

	[AllowNull]
	private Interval _currentHtfInterval;

	[AllowNull]
	private int[] _maPeriods;

	[AllowNull]
	private string[] _maLabels;

	[AllowNull]
	private HtfPlotSeries[] _maPlots;

	[AllowNull]
	private Indicator?[] _maIndicators;
	
	[AllowNull]
	private BarSeries _higherTimeframeBars;

	private bool IsHtfEmpty => _higherTimeframeBars.Count is 0;

	[Parameter("MA Timeframe", Description = "Timeframe of the MAs")]
	public Timeframe TimeframeValue { get; set; } = Timeframe.Day;

	[NumericRange(MinValue = 1)]
	[Parameter("MA Timeframe Bar Size", Description = "Bar size of the minute MAs")]
	public int MinuteBarSize { get; set; } = 240;

	[Parameter("MA Type", Description = "Type of the MAs")]
	public MaType MaTypeValue { get; set; } = MaType.Exponential;

	[Parameter("Show MAs Continuously", Description = "Whether MAs are shown with lines")]
	public bool ShowMasContinuously { get; set; } = true;

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

	public HtfPlotSeries MaPlot1 { get; init; }

	public HtfPlotSeries MaPlot2 { get; init; }

	public HtfPlotSeries MaPlot3 { get; init; }

	public HtfPlotSeries MaPlot4 { get; init; }

	public HtfPlotSeries MaPlot5 { get; init; }

	public HtfPlotSeries MaPlot6 { get; init; }

	public HtfPlotSeries MaPlot7 { get; init; }

	private DateTime GetStartTimeUtc()
	{
		var maMaxPeriod = _maPeriods.Max() + 1;

		var deltaTimeSpan = TimeframeValue switch
		{
			Timeframe.Day => TimeSpan.FromDays(1),
			Timeframe.Minute => TimeSpan.FromMinutes(MinuteBarSize),
			_ => throw new UnreachableException(),
		};

		var startTimeUtc = Bars.StartTimeUtc - maMaxPeriod * deltaTimeSpan;

		return startTimeUtc;
	}

	private string GetMaLabel(int maIndex)
	{
		var maLabel = _maLabels[maIndex];
		var maPeriod = _maPeriods[maIndex];

		return TimeframeValue switch
		{
			_ when !string.Equals(maLabel, _autoMaLabel, StringComparison.Ordinal) => maLabel,
			Timeframe.Day => $"MA {maPeriod}D",
			_ => $"MA {maPeriod}M",
		};
	}

	private double GetMaIncomingValue(int maIndex, int barIndex, int htfBarIndex, double maLastValue)
	{
		var currentClose = Bars.Close[barIndex];

		var maPeriod = _maPeriods[maIndex];
		var maIndicator = _maIndicators[maIndex];

		if (MaTypeValue is MaType.Exponential)
		{
			var maSmoothFactor = 2.0 / (1 + maPeriod);

			return currentClose * maSmoothFactor + maLastValue * (1 - maSmoothFactor);
        }

		if (MaTypeValue is MaType.Simple)
		{
			var smaPeriod = Math.Min(maPeriod - 1, htfBarIndex);

			var smaSum = Enumerable.Range(0, smaPeriod)
				.Select(_higherTimeframeBars.Close.Last)
				.Append(currentClose)
				.Sum();

			return smaSum / (1 + smaPeriod);
		}

		throw new UnreachableException();
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
		_currentHtfBarIndex = -1;

		_currentHtfInterval = new()
		{
			StartBarIndex = 0,
			EndBarIndex = 0,
		};

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

		var barSeriesInfo = new BarSeriesInfo
		{
			Period = barType,
			StartTimeUtc = startTimeUtc,
		};

		_higherTimeframeBars = GetBars(barSeriesInfo);

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maIndexCopy = maIndex;
			var maPlot = _maPlots[maIndex];
			var maPeriod = _maPeriods[maIndex];

			maPlot.IsVisible = ShowMasContinuously;
			maPlot.PriceMarker.Formatter = barIndex => GetMaLabel(maIndexCopy);

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
		var nextBarIndex = barIndex + 1;
		
		var currentClose = Bars.Close[barIndex];
		var currentTimeUtc = Bars.Time[barIndex];
		
		var htfBarIndex = _higherTimeframeBars.Count - 1;
		var isNewHtfBar = !_currentHtfBarIndex.Equals(htfBarIndex);

		var htfInterval = !isNewHtfBar
			? _currentHtfInterval
			: new Interval
			{
				StartBarIndex = nextBarIndex,
				EndBarIndex = nextBarIndex,
			};

		for (var maIndex = 0; maIndex < _maCount; maIndex++)
		{
			var maPlot = _maPlots[maIndex];
			var maIndicator = _maIndicators[maIndex];

			if (maIndicator is null || maIndicator.Plots is not [var maResult])
			{
				continue;
			}

			var maLastValue = IsHtfEmpty ? currentClose : maResult[htfBarIndex];

			maPlot.LastValue = GetMaIncomingValue(maIndex, barIndex, htfBarIndex, maLastValue);

			if (isNewHtfBar)
			{
				var htfLastBarIndex = Enumerable
					.Range(_currentHtfBarIndex + 1, htfBarIndex - _currentHtfBarIndex)
					.LastOrDefault(htfBarIndex => _higherTimeframeBars.Time[htfBarIndex] <= currentTimeUtc, htfBarIndex);

				var htfLastTimeUtc = _higherTimeframeBars.Time[htfLastBarIndex];

				if (Bars.GetBarIndex(htfLastTimeUtc) is var startBarIndex && startBarIndex is not -1)
				{
					for (var currentBarIndex = startBarIndex; currentBarIndex <= barIndex; currentBarIndex++)
					{
						maPlot[currentBarIndex] = maLastValue;
					}
				}
			}
		}

		_currentHtfInterval = htfInterval;
		_currentHtfBarIndex = htfBarIndex;
	}

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (_higherTimeframeBars.Count is not 0 && !ShowMasContinuously)
		{
			RenderDiscontinuousMas(drawingContext);	
		}
    }

	private void RenderDiscontinuousMas(IDrawingContext drawingContext)
    {
		for (var maIndex = 0; maIndex < _maCount; maIndex++)
        {
            var exchangeCalendar = Symbol.ExchangeCalendar;
            
			var visibleEndTimeUtc = Bars.Time[Chart.LastVisibleBarIndex];
            var visibleEndSession = exchangeCalendar.GetSession(visibleEndTimeUtc);

			if (visibleEndSession is null)
            {
                continue;
            }

            var maPlot = _maPlots[maIndex];
            var maValue = maPlot[Chart.LastVisibleBarIndex];
            var maValueY = ChartScale.GetYCoordinateByValue(maValue);
            
            var visibleStartX = Chart.GetLeftX();
            var visibleEndX = Chart.GetXCoordinateByTime(visibleEndTimeUtc);
            var visibleEndSessionStartX = Chart.GetXCoordinateByTime(visibleEndSession.StartUtcDateTime);

            double endLineX;
            double startLineX;
            
            if (AligmentValue is Aligment.Left)
            {
                startLineX = Math.Max(visibleStartX, visibleEndSessionStartX);

                endLineX = Math.Min(startLineX + MaxLineLengthInPixels, visibleEndX);
            }
            else
            {
                endLineX = visibleEndX;

                startLineX = Math.Max(endLineX - MaxLineLengthInPixels, visibleEndSessionStartX);
            }

            drawingContext.DrawHorizontalLine(startLineX, maValueY, endLineX, maPlot.Color);
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