using System.Diagnostics;
using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;
using Point = Tickblaze.Scripts.Api.Models.Point;

namespace Tickblaze.Scripts.Arc;

public partial class FairValueGaps : Indicator
{
    public FairValueGaps()
    {
        //_menu = new(this);

        IsOverlay = true;
        ShortName = "AFVG";
        Name = "ARC Fair Value Gaps";
    }

    //private readonly FairValueGapsMenu _menu;
    private AverageTrueRange _averageTrueRange;
    private readonly OrderedDictionary<int, Gap> _freshGaps = [];
    private readonly OrderedDictionary<int, Gap> _testedGaps = [];
    private readonly OrderedDictionary<int, Gap> _brokenGaps = [];
    
    [Parameter("Measurement", GroupName = "Parameters")]
    public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

    [NumericRange(MinValue = 1)]
    [Parameter("FVG Ticks", GroupName = "Parameters")]
    public int GapTickCount { get; set; } = 8;

    [NumericRange(MinValue = 0.01d, MaxValue = double.MaxValue, Step = 0.5d)]
    [Parameter("FVG ATR Multiple", GroupName = "Parameters")]
    public double AtrMultiple { get; set; } = 0.5d;

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Period", GroupName = "Parameters")]
    public int AtrPeriod { get; set; } = 14;

    [Parameter("Show Fresh FVGs", GroupName = "Visuals")]
    public bool ShowFreshGaps { get; set; } = true;

    [Parameter("Fresh FGV Color", GroupName = "Visuals")]
    public Color FreshGapColor { get; set; } = Color.New(Color.Orange, 0.5f);

    [Parameter("Show Tested FGVs", GroupName = "Visuals")]
    public bool ShowTestedGaps { get; set; } = true;

    [Parameter("Tested FGV Color", GroupName = "Visuals")]
    public Color TestedGapColor { get; set; } = Color.New(Color.Silver, 0.5f);

    [Parameter("Show Broken FGVs", GroupName = "Visuals")]
    public bool ShowBrokenGaps { get; set; } = true;

    [Parameter("Broken FGV Color", GroupName = "Visuals")]
    public Color BrokenGapColor { get; set; } = Color.New(Color.DimGray, 0.5f);

    [Parameter("Button Text", GroupName = "Visuals")]
    public string ButtonText { get; set; } = "TrapFinder";

    [Parameter("Enable Sounds", GroupName = "Alerts")]
    public bool AreSoundsEnabled { get; set; }

    [Parameter("Support FGV Hit WAV", GroupName = "Alerts")]
    public string? SupportGapHitWav { get; set; }

    [Parameter("Resistance FGV Hit WAV", GroupName = "Alerts")]
    public string? ResistanceGapHitWav { get; set; }

    // Todo: add plots.

    protected override Parameters GetParameters(Parameters parameters)
    {
        if (GapMeasurementValue is GapMeasurement.Atr)
        {
            parameters.Remove(nameof(GapTickCount));
        }
        
        if (GapMeasurementValue is GapMeasurement.Tick)
        {
            parameters.Remove(nameof(AtrPeriod));
            parameters.Remove(nameof(AtrMultiple));
        }

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

    //public override object? CreateChartToolbarMenuItem()
    //{
    //    return _menu;
    //}
    
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
        var minGapHeight = GapMeasurementValue switch
        {
            GapMeasurement.Atr => AtrMultiple * _averageTrueRange![index],
            GapMeasurement.Tick => GapTickCount * Symbol.TickSize,
            _ => throw new UnreachableException()
        };

        Gap[] gaps =
        [
            new()
            {
                IsSupport = true,
                FromIndex = index - 1,
                TopPrice = Bars.Low[index],
                BottomPrice = Bars.High[index - 2],
            },
            new()
            {
                IsSupport = false,
                FromIndex = index - 1,
                TopPrice = Bars.Low[index - 2],
                BottomPrice = Bars.High[index],
            },
        ];

        _freshGaps.Remove(index - 1);

        foreach (var gap in gaps)
        {
            if (gap.TopPrice - gap.BottomPrice > minGapHeight)
            {
                _freshGaps.Add(gap.FromIndex, gap);
            }
        }
    }

    private void CalculateTestedGaps(int index)
    {
        var lastBar = Bars[index]!;
        
        for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var (_, gap) = _freshGaps.GetAt(gapIndex);

            if (index - gap.FromIndex <= 1)
            {
                continue;
            }

            if (lastBar.Low < gap.TopPrice && gap.IsSupport
                || lastBar.High > gap.BottomPrice && gap.IsResistance)
            {
                gap.ToIndex = index;

                _freshGaps.RemoveAt(gapIndex);

                _testedGaps.Add(gap.FromIndex, gap);

                // Todo: sound alerts.
            }
        }
    }

    private void CalculateBrokenGaps(int index)
    {
        var lastBar = Bars[index]!;

        for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var (_, gap) = _testedGaps.GetAt(gapIndex);

            gapIndex--;

            if (lastBar.Low < gap.BottomPrice && gap.IsSupport
                || lastBar.High > gap.TopPrice && gap.IsResistance)
            {
                gapIndex++;
                
                gap.ToIndex = index;

                _testedGaps.RemoveAt(gapIndex);

                _brokenGaps.Add(gap.FromIndex, gap);
            }
        }
    }

    public override void OnRender(IDrawingContext context)
    {
        if (ShowFreshGaps)
        {
            RenderGaps(context, FreshGapColor, _freshGaps.Values);
        }

        if (ShowTestedGaps)
        {
            RenderGaps(context, TestedGapColor, _testedGaps.Values);
        }

        if (ShowBrokenGaps)
        {
            RenderGaps(context, BrokenGapColor, _brokenGaps.Values);
        }
    }

    private void RenderGaps(IDrawingContext drawingContext, Color fillColor, IEnumerable<Gap> gaps)
    {
        foreach (var gap in gaps)
        {
            var fromIndex = gap.FromIndex;
            var toIndex = gap.ToIndex ?? Math.Max(gap.FromIndex, Chart.LastVisibleBarIndex);

            if (toIndex - fromIndex >= 1
                && AreIntervalsIntersect(gap.BottomPrice, gap.TopPrice, ChartScale.MinPrice, ChartScale.MaxPrice)
                && AreIntervalsIntersect(fromIndex, toIndex, Chart.FirstVisibleBarIndex, Chart.LastVisibleBarIndex))
            {
                var fromXCoordinate = Chart.GetXCoordinateByBarIndex(fromIndex);
                var fromYCoordinate = ChartScale.GetYCoordinateByValue(gap.TopPrice);

                var toXCoordinate = Chart.GetXCoordinateByBarIndex(toIndex);
                var toYCoordinate = ChartScale.GetYCoordinateByValue(gap.BottomPrice);
                
                var topLeftPoint = new Point(fromXCoordinate, fromYCoordinate);
                var bottomRightPoint = new Point(toXCoordinate, toYCoordinate);

                drawingContext.DrawRectangle(topLeftPoint, bottomRightPoint, fillColor);
            }
        }
    }

    private static bool AreIntervalsIntersect(double firstIntervalStart,
        double firstIntervalEnd, double secondIntervalStart, double secondIntervalEnd)
    {
        return firstIntervalStart <= secondIntervalEnd && secondIntervalStart <= firstIntervalEnd;
    }
}
