using System.Diagnostics;

namespace Tickblaze.Scripts.Arc;

// Todo: parameter descriptions.
public partial class BigRoundNumbers : Indicator
{
	public BigRoundNumbers()
	{
		IsOverlay = true;
		ShortName = "TBC BRN";
		Name = "TB Core Big Round Numbers";
	}

	private double _intervalInPoints;

	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("Base Price")]
	public double BasePrice { get; set; }

	[Parameter("Level Color")]
	public Color LevelColor { get; set; } = DrawingColor.Navy.ToApiColor();

	[NumericRange(MinValue = 1, MaxValue = 10)]
	[Parameter("Level Thickness")]
	public int LevelThickness { get; set; } = 2;

	[Parameter("Interval Price")]
	public IntervalType IntervalTypeValue { get; set; } = IntervalType.Points;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Pts")]
	public int IntervalInPoints { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Ticks")]
	public int IntervalInTicks { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Pips")]
	public int IntervalInPips { get; set; } = 1;

	[Parameter("Highlight Color", GroupName = "Level Visuals")]
	public Color HighlightColor { get; set; } = DrawingColor.Gold.ToApiColor(0.0f);

	[Parameter("Highlight Thickness Type", GroupName = "Level Visuals")]
	public HighlightRegionHeightType HighlightThicknessTypeValue { get; set; } = HighlightRegionHeightType.Ticks;

	[NumericRange(MinValue = 1)]
	[Parameter("Highlight Thickness Ticks", GroupName = "Level Visuals")]
	public int HighlightRegionHeightInTicks { get; set; } = 1;

	[NumericRange(MinValue = 1)]
	[Parameter("Highlight Thickness Pixels", GroupName = "Level Visuals")]
	public int HighlightRegionHeightInPixels { get; set; } = 5;

	protected override Parameters GetParameters(Parameters parameters)
	{
		List<string> propertyNames =
		[
			nameof(IntervalInPoints),
			nameof(IntervalInTicks),
			nameof(IntervalInPips),
		];

		var propertyName = IntervalTypeValue switch
		{
			IntervalType.Points => nameof(IntervalInPoints),
			IntervalType.Ticks => nameof(IntervalInTicks),
			IntervalType.Pips => nameof(IntervalInPips),
			_ => throw new UnreachableException(),
		};

		propertyNames.Remove(propertyName);

		parameters.RemoveRange(propertyNames);

		propertyName = HighlightThicknessTypeValue switch
		{
			HighlightRegionHeightType.Points => nameof(HighlightRegionHeightInTicks),
			HighlightRegionHeightType.Ticks => nameof(HighlightRegionHeightInPixels),
			_ => throw new UnreachableException(),
		};

		parameters.Remove(propertyName);

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
		var regionHeight = GetRegionHeight();
		var priceLevel = GetFirstBigRoundNumber();

		while (priceLevel <= maxPrice)
		{
			var priceY = ChartScale.GetYCoordinateByValue(priceLevel);

			if (HighlightColor.A is not 0)
			{
				var startRegionPoint = new Point(0, priceY - regionHeight / 2.0);

				context.DrawRectangle(startRegionPoint, Chart.Width, regionHeight, HighlightColor);
			}
			
			context.DrawHorizontalLine(0, priceY, Chart.Width, LevelColor, LevelThickness);

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