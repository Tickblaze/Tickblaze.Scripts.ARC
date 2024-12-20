using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public partial class FairValueGaps : Indicator
{
    public FairValueGaps()
    {
        _menu = new(this);
    }

    private readonly FairValueGapsMenu _menu;
    private readonly List<Gap> _freshGaps = [];
    private readonly List<Gap> _testedGaps = [];
    private readonly List<Gap> _brokenGaps = [];
    private AverageTrueRange _averageTrueRange = default!;
    
    [Parameter("Measurement", GroupName = "Parameters")]
    public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

    [Parameter("FVG Ticks", GroupName = "Parameters")]
    [NumericRange(MinValue = 1)]
    public int GapTickCount { get; set; } = 8;

    [Parameter("FVG ATR Multiple", GroupName = "Parameters")]
    [NumericRange(MinValue = 0.01d, MaxValue = double.MaxValue)]
    public double AtrMultiple { get; set; } = 0.5d;

    [Parameter("ATR Period", GroupName = "Parameters")]
    [NumericRange(MinValue = 1)]
    public int AtrPeriod { get; set; } = 14;

    [Parameter("Show Fresh FVGs", GroupName = "Visuals")]
    public bool ShowFreshFvgs { get; set; } = true;

    [Parameter("Fresh FGV Color", GroupName = "Visuals")]
    public Color FreshFvgColor { get; set; } = Color.New(Color.Orange, 0.5f);

    [Parameter("Show Tested FGVs", GroupName = "Visuals")]
    public bool ShowTestedFvgs { get; set; } = true;

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

        if (!ShowFreshFvgs)
        {
            parameters.Remove(nameof(FreshFvgColor));
        }

        if (!ShowTestedFvgs)
        {
            parameters.Remove(nameof(TestedGapColor));
        }

        if (!ShowBrokenGaps)
        {
            parameters.Remove(nameof(BrokenGapColor));
        }

        return parameters;
    }

    public override object? CreateChartToolbarMenuItem()
    {
        return _menu;
    }
    
    protected override void Initialize()
    {
        _averageTrueRange = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
    }

    protected override void Calculate(int index)
    {
        if (index <= 2)
        {
            return;
        }

        CalculateFreshGaps(index);
        CalculateTestedGaps(index);
        CalculateBrokenGaps(index);
    }

    private void CalculateFreshGaps(int index)
    {
        var minFvgHeight = GapMeasurementValue is GapMeasurement.Atr
            ? _averageTrueRange[index] * AtrMultiple
            : GapTickCount * Symbol.TickSize;

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

        foreach (var gap in gaps)
        {
            if (gap.TopPrice - gap.BottomPrice > minFvgHeight)
            {
                _freshGaps.Add(gap);
            }
        }
    }

    private void CalculateTestedGaps(int index)
    {
        for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var lastBar = Bars[index]!;
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
        for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
        {
            var lastBar = Bars[index]!;
            var fairValueGap = _freshGaps[gapIndex];

            if (lastBar.High >= fairValueGap.TopPrice && fairValueGap.IsResistance
                || lastBar.Low <= fairValueGap.BottomPrice && fairValueGap.IsSupport)
            {
                fairValueGap.ToIndex = index;

                _testedGaps.RemoveAt(gapIndex);

                _brokenGaps.Add(fairValueGap);

                gapIndex++;
            }
        }
    }

    public override void OnRender(IDrawingContext context)
    {
        if (ShowFreshFvgs)
        {
            RenderGaps(context, FreshFvgColor, _freshGaps);
        }

        if (ShowTestedFvgs)
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
