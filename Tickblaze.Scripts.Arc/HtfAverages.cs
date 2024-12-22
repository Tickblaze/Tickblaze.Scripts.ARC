using System.Diagnostics;
using Tickblaze.Scripts.Indicators;
using DrawingColor = System.Drawing.Color;

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
    private const string _autoLabel = "<auto>";

    private BarSeries _higherTimeframeBars = default!;
    private ISeries<double>?[] _movingAverages = new ISeries<double>?[_maCount];

    [Parameter("Bkg Timeframe", GroupName = "Parameters")]
    public Timeframe TimeframeValue { get; set; } = Timeframe.Day;

    [NumericRange(MinValue = 1)]
    [Parameter("Bkg Time Value", GroupName = "Parameters")]
    public int TimeInMinutes { get; set; } = 240;

    [Parameter("MA Type", GroupName = "Parameters")]
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
    public LineOrigin HorizontalLineAligmentValue { get; set; } = LineOrigin.FromLeft;

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 1")]
    public int MaPeriod1 { get; set; } = 6;

    [Parameter("MA Label", GroupName = "Moving Average 1")]
    public string MaLabel1 { get; set; } = _autoLabel;

    [Plot("Plot1")]
    public PlotSeries MaPlot1 { get; set; } = new(Color.FromDrawingColor(DrawingColor.Goldenrod));

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 2")]
    public int MaPeriod2 { get; set; } = 12;

    [Parameter("MA Label", GroupName = "Moving Average 2")]
    public string MaLabel2 { get; set; } = _autoLabel;

    [Plot("Plot2")]
    public PlotSeries MaPlot2 { get; set; } = new(Color.White);

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 3")]
    public int MaPeriod3 { get; set; } = 25;

    [Parameter("MA Label", GroupName = "Moving Average 3")]
    public string MaLabel3 { get; set; } = _autoLabel;

    [Plot("Plot3")]
    public PlotSeries MaPlot3 { get; set; } = new(Color.Yellow);

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 4")]
    public int MaPeriod4 { get; set; } = 50;

    [Parameter("MA Label", GroupName = "Moving Average 4")]
    public string MaLabel4 { get; set; } = _autoLabel;

    [Plot("Plot4")]
    public PlotSeries MaPlot4 { get; set; } = new(Color.Black);

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 5")]
    public int MaPeriod5 { get; set; } = 100;

    [Parameter("MA Label", GroupName = "Moving Average 5")]
    public string MaLabel5 { get; set; } = _autoLabel;

    [Plot("Plot5")]
    public PlotSeries MaPlot5 { get; set; } = new(Color.Red);

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 6")]
    public int MaPeriod6 { get; set; }

    [Parameter("MA Label", GroupName = "Moving Average 6")]
    public string MaLabel6 { get; set; } = _autoLabel;

    [Plot("Plot6")]
    public PlotSeries MaPlot6 { get; set; } = new(Color.Cyan);

    [NumericRange]
    [Parameter("MA Period", GroupName = "Moving Average 7")]
    public int MaPeriod7 { get; set; }

    [Parameter("MA Label", GroupName = "Moving Average 7")]
    public string MaLabel7 { get; set; } = _autoLabel;

    [Plot("Plot7")]
    public PlotSeries MaPlot7 { get; set; } = new(Color.FromDrawingColor(DrawingColor.Lime));

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

            Symbol = Bars.Symbol,
            Period = GetBarType(TimeframeValue, TimeInMinutes),
        };

        _higherTimeframeBars = GetBarSeries(barSeriesRequest);

        for (var maIndex = 0; maIndex < _maCount; maIndex++)
        {
            var maPeriod = maPeriods[maIndex];

            _movingAverages[maIndex] = null;

            if (maPeriod is not 0)
            {
                _movingAverages[maIndex] = GetMovingAverageSeries(_higherTimeframeBars.Close, maPeriod, MovingAverageTypeValue);
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
            var movingAverage = _movingAverages[maIndex];

            maPlot[index] = movingAverage is not { Count: >= 1 } ? double.NaN : movingAverage.Last();
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

    private static ISeries<double> GetMovingAverageSeries(ISeries<double> bars, int period, MovingAverageType movingAverageType)
    {
        Indicator movingAverage = movingAverageType switch
        {
            MovingAverageType.Simple => new SimpleMovingAverage(bars, period),
            MovingAverageType.Exponential => new ExponentialMovingAverage(bars, period),
            _ => throw new UnreachableException(),
        };

        return movingAverage.Plots.Single();
    }
}
