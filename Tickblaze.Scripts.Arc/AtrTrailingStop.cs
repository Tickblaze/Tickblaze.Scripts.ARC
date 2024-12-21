namespace Tickblaze.Scripts.Arc;

/// <summary>
/// ARC ATRTrailingStop [AATS]
/// </summary>
public partial class AtrTrailingStop : Indicator
{
    public AtrTrailingStop()
    {
        IsOverlay = true;
        ShortName = "AATS";
        Name = "ARC ATR Trailing Stop";
    }

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Period", GroupName = "Input Parameters")]
    public int AtrPeriod { get; set; } = 14;

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Multiplier", GroupName = "Input Parameters")]
    public int AtrMultiplier { get; set; } = 3;

    [Parameter("Show stop dots", GroupName = "Display Options")]
    public bool ShowStopDots { get; set; } = true;

    [Parameter("Show stop line", GroupName = "Display Options")]
    public bool ShowStopLine { get; set; } = true;

    [Parameter("Show Triangles", GroupName = "Display Options")]
    public bool ShowMarkers { get; set; }

    [Parameter("Bullish Color", GroupName = "Plots")]
    public Color BullishColor { get; set; } = Color.Blue;

    [Parameter("Bearish Color", GroupName = "Plots")]
    public Color BearishColor { get; set; } = Color.Red;

    [Parameter("Plotstyle stop dots", GroupName = "Plots")]
    public PlotStyle PlotStyleStopDots { get; set; } = PlotStyle.Dot;

    public PlotSeries StopDots { get; set; } = new();

    public PlotSeries StopLine { get; set; } = new();

    public PlotSeries StopMarkers { get; set; } = new();

    protected override void Initialize()
    {

    }

    protected override void Calculate(int index)
    {
        
    }
}
