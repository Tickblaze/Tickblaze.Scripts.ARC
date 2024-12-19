using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public sealed class FairValueGaps : Indicator
{
    public FairValueGaps()
    {

    }

    private AverageTrueRange _averageTrueRange = default!;

    private readonly List<FairValueGap> _freshFvgs = [];
    private readonly List<FairValueGap> _testedFvgs = [];
    private readonly List<FairValueGap> _brokenFvgs = [];

    // Todo: add parameters.

    [Parameter("Measurement")]
    public GapMeasurement Measurement { get; set; } = GapMeasurement.Atr;

    [Parameter("FVG Ticks")]
    public int FvgTickCount { get; set; } = 8;

    [Parameter("Trap ATR Multiple")]
    public double AtrMultiple { get; set; } = 0.5d;

    [Parameter("ATR Period")]
    public int AtrPeriod { get; set; } = 14;

    [Parameter("Show Fresh FVGs")]
    public bool ShowFreshFvgs { get; set; } = true;

    [Parameter("Fresh FGV Brush")]
    public Color FreshFvgColor { get; set; } = Color.New(Color.Orange, 0.5f);

    [Parameter("Show Tested FGVs")]
    public bool ShowTestedFvgs { get; set; } = true;

    [Parameter("Tested FGV Color")]
    public Color TestedFvgColor { get; set; } = Color.New(Color.Silver, 0.5f);

    [Parameter("Show Broken FGVs")]
    public bool ShowBrokenFvgs { get; set; } = true;

    [Parameter("Broken FGV Brush")]
    public Color BrokenFvgColor { get; set; } = Color.New(Color.DimGray, 0.5f);

    [Parameter("Button Text")]
    public string ButtonText { get; set; } = "TrapFinder";

    [Parameter("Enable Sounds")]
    public bool AreSoundsEnabled { get; set; }

    [Parameter("Support FGV Hit WAV")]
    public string? SupportFvgHitWav { get; set; }

    [Parameter("Resistance FGV Hit WAV")]
    public string? ResistanceFvgHitWav { get; set; }

    public override object? CreateChartToolbarMenuItem()
    {
        return base.CreateChartToolbarMenuItem();
    }

    protected override void Initialize()
    {
        // Todo: initialize indicator bars.
        _averageTrueRange = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
    }

    protected override void Calculate(int index)
    {
        base.Calculate(index);

        if (index <= 2)
        {
            return;
        }

        CalculateFreshFvgs(index);
        CalculateTestedFvgs(index);
        CalculateBrokenFvgs(index);
    }

    private void CalculateFreshFvgs(int index)
    {
        var minFvgHeight = Measurement is GapMeasurement.Atr
            ? _averageTrueRange[index] * AtrMultiple
            : FvgTickCount * Symbol.TickSize;

        ReadOnlySpan<FairValueGap> fairValueGaps =
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

        foreach (var fairValueGap in fairValueGaps)
        {
            if (fairValueGap.TopPrice - fairValueGap.BottomPrice > minFvgHeight)
            {
                _freshFvgs.Add(fairValueGap);
            }
        }
    }

    private void CalculateTestedFvgs(int index)
    {
        for (var fvgIndex = _freshFvgs.Count - 1; fvgIndex >= 0; fvgIndex--)
        {
            var lastBar = Bars[index]!;
            var fairValueGap = _freshFvgs[fvgIndex];

            if (fairValueGap.IsInGap(lastBar.Low) || fairValueGap.IsInGap(lastBar.High))
            {
                fairValueGap.ToIndex = index;

                _freshFvgs.RemoveAt(fvgIndex);

                _testedFvgs.Add(fairValueGap);

                fvgIndex++;

                // Todo: sound alerts.
            }
        }
    }

    private void CalculateBrokenFvgs(int index)
    {
        for (var fvgIndex = _testedFvgs.Count - 1; fvgIndex >= 0; fvgIndex--)
        {
            var lastBar = Bars[index]!;
            var fairValueGap = _freshFvgs[fvgIndex];

            if (lastBar.High >= fairValueGap.TopPrice && fairValueGap.IsResistance
                || lastBar.Low <= fairValueGap.BottomPrice && fairValueGap.IsSupport)
            {
                fairValueGap.ToIndex = index;

                _testedFvgs.RemoveAt(fvgIndex);

                _brokenFvgs.Add(fairValueGap);

                fvgIndex++;
            }
        }
    }

    public override void OnRender(IDrawingContext context)
    {
        if (ShowFreshFvgs)
        {
            RenderFvgs(context, FreshFvgColor, _freshFvgs);
        }

        if (ShowTestedFvgs)
        {
            RenderFvgs(context, TestedFvgColor, _testedFvgs);
        }

        if (ShowBrokenFvgs)
        {
            RenderFvgs(context, BrokenFvgColor, _brokenFvgs);
        }
    }

    private void RenderFvgs(IDrawingContext drawingContext, Color fillColor, List<FairValueGap> fairValueGaps)
    {
        foreach (var fairValueGap in fairValueGaps)
        {
            var fromIndex = fairValueGap.FromIndex;
            var toIndex = fairValueGap.ToIndex ?? Math.Max(fairValueGap.FromIndex, Chart.LastVisibleBarIndex);

            if (toIndex - fromIndex >= 1
                && fromIndex <= Chart.LastVisibleBarIndex
                && Chart.FirstVisibleBarIndex <= toIndex)
            {
                var fromCoordinate = Chart.GetXCoordinateByBarIndex(fromIndex);
                var toCoordinate = Chart.GetXCoordinateByBarIndex(toIndex);

                var topLeftPoint = new Point(fromCoordinate, fairValueGap.TopPrice);
                var bottomRightPoint = new Point(toCoordinate, fairValueGap.BottomPrice);

                drawingContext.DrawRectangle(topLeftPoint, bottomRightPoint, fillColor);
            }
        }
    }
}
