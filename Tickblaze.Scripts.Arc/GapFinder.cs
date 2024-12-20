using System.Diagnostics;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

// Note: maybe to move common logic to a base class [if that would simplify things].

public partial class GapFinder : Indicator
{
    public GapFinder()
    {
        IsOverlay = true;
        ShortName = "AGF";
        Name = "ARC Gap Finder";
    }

    private readonly List<Gap> _freshGaps = [];
    private readonly List<Gap> _testedGaps = [];
    private readonly List<Gap> _brokenGaps = [];
    private AverageTrueRange _averageTrueRange = default!;

    [Parameter("Measurement", GroupName = "Parameters")]
    public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Ticks", GroupName = "Parameters")]
    public int GapTickCount { get; set; } = 8;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Pts", GroupName = "Parameters")]
    public int GapPointCount { get; set; } = 5;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Pts", GroupName = "Parameters")]
    public int GapPipCount { get; set; } = 20;

    [NumericRange(MinValue = 0.01, MaxValue = double.MaxValue)]
    [Parameter("Gap ATR Multiple", GroupName = "Parameters")]
    public double AtrMultiple { get; set; } = 0.5;

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Period", GroupName = "Parameters")]
    public int AtrPeriod { get; set; } = 14;

    [Parameter("Restrict to New Session", GroupName = "Parameters")]
    public bool IsRestrictedToNewSessions { get; set; }

    [Parameter("Show Fresh Gaps", GroupName = "Visuals")]
    public bool ShowFreshGaps { get; set; }

    [Parameter("Fresh Gap Color", GroupName = "Visuals")]
    public Color FreshGapColor { get; set; } = Color.New(Color.Orange, 0.5f);

    [Parameter("Show Tested Gaps", GroupName = "Visuals")]
    public bool ShowTestedGaps { get; set; }

    [Parameter("Tested Gap Color", GroupName = "Visuals")]
    public Color TestedGapColor { get; set; } = Color.New(Color.Silver, 0.5f);

    [Parameter("Show Broken Gaps", GroupName = "Visuals")]
    public bool ShowBrokenGaps { get; set; }

    [Parameter("Broken Gap Color", GroupName = "Visuals")]
    public Color BrokenGapColor { get; set; } = Color.New(Color.DimGray, 0.5f);

    [Parameter("Button Text", GroupName = "Visuals")]
    public string ButtonText { get; set; } = "GapFinder";

    [Parameter("Enable Sounds", GroupName = "Alerts")]
    public bool AreSoundEnabled { get; set; }

    [Parameter("Gap Hit WAV", GroupName = "Alerts")]
    public string? GapHitWav { get; set; }

    private bool IsNewSessionBar(int index)
    {
        var exchangeCalendar = Bars.Symbol.ExchangeCalendar;

        var currentBarTimeUtc = Bars.Time[index];
        var previousBarTimeUtc = Bars.Time[index - 1];

        var currentSession = exchangeCalendar.GetSession(currentBarTimeUtc);
        var previousSession = exchangeCalendar.GetSession(previousBarTimeUtc);

        return currentSession is not null
            && previousSession is not null
            && DateTime.Equals(currentSession.StartUtcDateTime, previousSession.StartUtcDateTime);
    }

    protected override Parameters GetParameters(Parameters parameters)
    {
        List<string> gapSizePropertyNames =
        [
            nameof(GapTickCount),
            nameof(GapPointCount),
            nameof(GapPipCount),
            nameof(AtrMultiple),
            nameof(AtrPeriod),
        ];

        var _ = GapMeasurementValue switch
        {
            GapMeasurement.Tick => gapSizePropertyNames.Remove(nameof(GapTickCount)),
            GapMeasurement.Point => gapSizePropertyNames.Remove(nameof(GapPointCount)),
            GapMeasurement.Pip => gapSizePropertyNames.Remove(nameof(GapPipCount)),
            GapMeasurement.Atr => gapSizePropertyNames.Remove(nameof(AtrMultiple))
                & gapSizePropertyNames.Remove(nameof(AtrPeriod)),
            _ => throw new UnreachableException()
        };

        gapSizePropertyNames.ForEach(propertyName => parameters.Remove(propertyName));

        if (!ShowFreshGaps)
        {
            parameters.Remove(nameof(FreshGapColor));
        }

        if (!ShowTestedGaps)
        {
            parameters.Remove(nameof(TestedGapColor));
        }

        if (!ShowBrokenGaps)
        {
            parameters.Remove(nameof(BrokenGapColor));
        }

        return parameters;
    }

    protected override void Initialize()
    {
        _averageTrueRange = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
    }

    protected override void Calculate(int index)
    {
        if (index <= 1)
        {
            return;
        }

        CalculateFreshGaps(index);
        CalculateTestedGaps(index);
        CalculateBrokenGaps(index);
    }

    private void CalculateFreshGaps(int index)
    {
        if (IsRestrictedToNewSessions && !IsNewSessionBar(index))
        {
            return;
        }

        var tickSize = Symbol.TickSize;
        var minGapHeight = GapMeasurementValue switch
        {
            GapMeasurement.Point => GapPointCount,
            GapMeasurement.Pip => 10 * GapPipCount * tickSize,
            GapMeasurement.Tick => GapTickCount * tickSize,
            GapMeasurement.Atr => AtrMultiple * _averageTrueRange[index],
            _ => throw new UnreachableException()
        };

        Gap[] gaps =
        [
            new()
            {
                IsSupport = true,
                FromIndex = index - 1,
                TopPrice = Bars.Open[index],
                BottomPrice = Bars.Close[index - 1],
            },
            new()
            {
                IsSupport = false,
                FromIndex = index - 1,
                TopPrice = Bars.Close[index - 1],
                BottomPrice = Bars.Open[index],
            },
        ];

        foreach (var gap in gaps)
        {
            if (gap.TopPrice - gap.BottomPrice > minGapHeight)
            {
                _freshGaps.Add(gap);
            }
        }
    }

    private void CalculateTestedGaps(int index)
    {
        var lastBar = Bars[index]!;
        
        for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var gap = _freshGaps[gapIndex];

            if (gap.IsInPriceGap(lastBar.Low) || gap.IsInPriceGap(lastBar.High))
            {
                gap.ToIndex = index;

                _freshGaps.RemoveAt(gapIndex);

                _testedGaps.Add(gap);

                gapIndex++;

                // Todo: sound alerts.
            }
        }
    }

    private void CalculateBrokenGaps(int index)
    {
        var lastBar = Bars[index]!;

        for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var gap = _testedGaps[gapIndex];

            if (lastBar.High >= gap.TopPrice && gap.IsResistance
                || lastBar.Low <= gap.BottomPrice && gap.IsSupport)
            {
                gap.ToIndex = index;

                _testedGaps.RemoveAt(gapIndex);

                _brokenGaps.Add(gap);

                gapIndex++;
            }
        }
    }

    public override void OnRender(IDrawingContext context)
    {
        if (ShowFreshGaps)
        {
            RenderGaps(context, FreshGapColor, _freshGaps);
        }

        if (ShowTestedGaps)
        {
            RenderGaps(context, TestedGapColor, _testedGaps);
        }

        if (ShowBrokenGaps)
        {
            RenderGaps(context, BrokenGapColor, _brokenGaps);
        }
    }

    private void RenderGaps(IDrawingContext drawingContext, Color fillColor, List<Gap> gaps)
    {
        foreach (var gap in gaps)
        {
            var fromIndex = gap.FromIndex;
            var toIndex = gap.ToIndex ?? Math.Max(gap.FromIndex, Chart.LastVisibleBarIndex);

            if (toIndex - fromIndex >= 1
                && fromIndex <= Chart.LastVisibleBarIndex
                && Chart.FirstVisibleBarIndex <= toIndex)
            {
                var fromCoordinate = Chart.GetXCoordinateByBarIndex(fromIndex);
                var toCoordinate = Chart.GetXCoordinateByBarIndex(toIndex);

                var topLeftPoint = new Point(fromCoordinate, gap.TopPrice);
                var bottomRightPoint = new Point(toCoordinate, gap.BottomPrice);

                drawingContext.DrawRectangle(topLeftPoint, bottomRightPoint, fillColor);
            }
        }
    }
}
