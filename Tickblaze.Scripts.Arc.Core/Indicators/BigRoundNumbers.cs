using System.Diagnostics;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class BigRoundNumbers : Indicator
{
	public BigRoundNumbers()
	{
		IsOverlay = true;
		
		ShortName = "BRN";
		
		Name = "Big Round Numbers";
	}

	private double _intervalInPoints;

	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("Base Price", Description = "Price from which all lines eminate at interval values")]
	public double BasePrice { get; set; }

	[Parameter("Interval Type", Description = "Type of line distance measurement")]
	public IntervalType IntervalTypeValue { get; set; } = IntervalType.Points;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Points", Description = "Distance between the lines in points")]
	public int IntervalInPoints { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Ticks", Description = "Distance between the lines in ticks")]
	public int IntervalInTicks { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("Interval in Pips", Description = "Distance between the lines in pips = 10 * ticks")]
	public int IntervalInPips { get; set; } = 1;

	[NumericRange(MinValue = 1, MaxValue = 10)]
	[Parameter("Level Thickness", GroupName = "Level Visuals", Description = "Thickness of the level line")]
	public int LevelThickness { get; set; } = 2;

	[Parameter("Highlight Thickness Type", GroupName = "Level Visuals", Description = "Type of the highlighted region height measurement")]
	public HighlightRegionHeightType HighlightThicknessTypeValue { get; set; } = HighlightRegionHeightType.Ticks;

	[NumericRange(MinValue = 1)]
	[Parameter("Highlight Thickness Ticks", GroupName = "Level Visuals", Description = "Height of the highlighted region in ticks")]
	public int HighlightGapHeightInTicks { get; set; } = 1;

	[NumericRange(MinValue = 1)]
	[Parameter("Highlight Thickness Pixels", GroupName = "Level Visuals", Description = "Height of the highlighted region in pixels")]
	public int HighlightGapHeightInPixels { get; set; } = 5;

	[Parameter("Level Color", GroupName = "Level Visuals", Description = "Color of the level line")]
	public Color LevelColor { get; set; } = DrawingColor.Navy;

	[Parameter("Highlight Color", GroupName = "Level Visuals", Description = "Color of the highlighted region")]
	public Color HighlightColor { get; set; } = DrawingColor.Gold.With(0.0f);

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

		if (HighlightThicknessTypeValue is HighlightRegionHeightType.Pixels)
		{
			parameters.Remove(nameof(HighlightGapHeightInTicks));
		}

		if (HighlightThicknessTypeValue is HighlightRegionHeightType.Ticks)
		{
			parameters.Remove(nameof(HighlightGapHeightInPixels));
		}

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
				var startRegionPoint = new ApiPoint(0, priceY - regionHeight / 2.0);

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
			HighlightRegionHeightType.Pixels => HighlightGapHeightInPixels,
			HighlightRegionHeightType.Ticks => HighlightGapHeightInTicks * tickHeight,
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

	public enum IntervalType
	{
		[DisplayName("Points")]
		Points,

		[DisplayName("Ticks")]
		Ticks,

		[DisplayName("Pips")]
		Pips
	};

	public enum HighlightRegionHeightType
	{
		[DisplayName("Ticks")]
		Ticks,

		[DisplayName("Pixels")]
		Pixels,
	}
}