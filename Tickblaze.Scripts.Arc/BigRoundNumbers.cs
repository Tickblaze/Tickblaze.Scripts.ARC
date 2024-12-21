using System.Diagnostics;
using DrawingColor = System.Drawing.Color;

namespace Tickblaze.Scripts.Arc;

/// <summary>
/// ARC_BigRoundNumbers [ABRN]
/// </summary>
public partial class BigRoundNumbers : Indicator
{
    public BigRoundNumbers()
    {
        IsOverlay = true;
        ShortName = "ABRN";
        Name = "ARC Big Round Numbers";
    }

    private double _intervalInPoints;

    [NumericRange(MaxValue = double.MaxValue)]
    [Parameter("Base Price", GroupName = "Parameters")]
    public double BasePrice { get; set; }

    [Parameter("Interval Price", GroupName = "Parameters")]
    public IntervalType IntervalTypeValue { get; set; } = IntervalType.Points;

    [NumericRange(MinValue = 1)]
    [Parameter("Interval in Pts", GroupName = "Parameters")]
    public int IntervalInPoints { get; set; } = 10;

    [NumericRange(MinValue = 1)]
    [Parameter("Interval in Ticks", GroupName = "Parameters")]
    public int IntervalInTicks { get; set; } = 10;

    [NumericRange(MinValue = 1)]
    [Parameter("Interval in Pips", GroupName = "Parameters")]
    public int IntervalInPips { get; set; } = 1;

    [Parameter("Highlight Color", GroupName = "Level Visuals")]
    public Color HighlightColor { get; set; } = Color.New(Color.FromDrawingColor(DrawingColor.Gold), 0);

    [Parameter("Highlight Thickness Type", GroupName = "Level Visuals")]
    public HighlightRegionHeightType HighlightThicknessTypeValue { get; set; } = HighlightRegionHeightType.Ticks;

    [NumericRange(MinValue = 1)]
    [Parameter("Highlight Thickness Ticks", GroupName = "Level Visuals")]
    public int HighlightRegionHeightInTicks { get; set; } = 1;

    [NumericRange(MinValue = 1)]
    [Parameter("Highlight Thickness Pixels", GroupName = "Level Visuals")]
    public int HighlightRegionHeightInPixels { get; set; } = 5;

    // Question: is it relevant?
    // [Parameter("Draw as HLine Objects", GroupName = "Level Visuals")]
    // public bool ArePlotSettingsUsed { get; set; }

    [Plot("Lvl")]
    public PlotSeries Level { get; set; } = new(Color.FromDrawingColor(DrawingColor.Navy));

    protected override Parameters GetParameters(Parameters parameters)
    {
        List<string> intervalPropertyNames =
        [
            nameof(IntervalInPoints),
            nameof(IntervalInTicks),
            nameof(IntervalInPips),
        ];

        var intervalPropertyName = IntervalTypeValue switch
        {
            IntervalType.Points => nameof(IntervalInPoints),
            IntervalType.Ticks => nameof(IntervalInTicks),
            IntervalType.Pips => nameof(IntervalInPips),
            _ => throw new UnreachableException()
        };

        intervalPropertyNames.Remove(intervalPropertyName);

        intervalPropertyNames.ForEach(propertyName => parameters.Remove(propertyName));

        var _ = HighlightThicknessTypeValue switch
        {
            HighlightRegionHeightType.Points => parameters.Remove(nameof(HighlightRegionHeightInTicks)),
            HighlightRegionHeightType.Ticks => parameters.Remove(nameof(HighlightRegionHeightInPixels)),
            _ => throw new UnreachableException()
        };

        return parameters;
    }

    protected override void Initialize()
    {
        var tickSize = Bars.Symbol.TickSize;

        _intervalInPoints = IntervalTypeValue switch
        {
            IntervalType.Points => Math.Max(IntervalInPoints, tickSize),
            IntervalType.Ticks => IntervalInTicks * tickSize,
            IntervalType.Pips => 10 * IntervalInPips * tickSize,
            _ => throw new UnreachableException(),
        };
    }

    public override void OnRender(IDrawingContext context)
    {
        var maxPrice = ChartScale.MaxPrice;
        var priceLevel = GetFirstBigRoundNumber();
        var regionHeight = GetRegionHeight();

        while (priceLevel <= maxPrice)
        {
            var yCoordinate = ChartScale.GetYCoordinateByValue(priceLevel);

            if (HighlightColor.A is not 0)
            {
                var startRegionPoint = new Point(0, yCoordinate - regionHeight / 2.0);

                context.DrawRectangle(startRegionPoint, Chart.Width, regionHeight, HighlightColor);
            }
            
            var startPoint = new Point(0, yCoordinate);
            var endPoint = new Point(Chart.Width, yCoordinate);

            context.DrawLine(startPoint, endPoint, Level.Color, Level.Thickness);

            priceLevel += _intervalInPoints;
        }
    }

    private double GetRegionHeight()
    {
        var tickSize = Bars.Symbol.TickSize;
        var tickHeight = ChartScale.GetYCoordinateByValue(tickSize) - ChartScale.GetYCoordinateByValue(0);

        return HighlightThicknessTypeValue switch
        {
            HighlightRegionHeightType.Points => HighlightRegionHeightInPixels,
            HighlightRegionHeightType.Ticks => HighlightRegionHeightInTicks * tickHeight,
            _ => throw new UnreachableException(),
        };
    }

    private double GetFirstBigRoundNumber()
    {
        var minPrice = ChartScale.MinPrice;

        if (minPrice >= BasePrice)
        {
            var intervalMultiplier = (minPrice - BasePrice) / _intervalInPoints;

            return BasePrice + Math.Floor(intervalMultiplier) * _intervalInPoints;
        }
        else
        {
            var intervalMultiplier = (BasePrice - minPrice) / _intervalInPoints;

            return BasePrice - Math.Ceiling(intervalMultiplier) * _intervalInPoints;
        }
    }
}