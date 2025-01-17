using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[AllowNull]
	private SimpleMovingAverage _priceExcursion;

	[AllowNull]
	private AverageTrueRange _priceExcursionAtr;

	private PlotSeries PriceExcursion => _priceExcursion.Result;

	public bool EnableLevels { get; set; } = true;

	[Parameter("Level Plot Style", GroupName = "Price Excursion Visuals", Description = "Plot style of the level lines")]
	public LevelPlotStyle LevelPlotStyleValue { get; set; } = LevelPlotStyle.StraightLines;

	[Parameter("Level Line Style", GroupName = "Price Excursion Visuals", Description = "Line style of the level lines")]
	public LineStyle LevelLineStyle { get; set; } = LineStyle.Solid;

	[NumericRange(MinValue = 1)]
	[Parameter("Level Line Thickness", GroupName = "Price Excursion Visuals", Description = "Thickness of the level lines")]
	public int LevelLineThickness { get; set; } = 3;

	[Parameter("Show Level 1", GroupName = "Price Excursion Visuals", Description = "Whether level 1 lines are shown")]
	public bool ShowLevel1 { get; set; }

	[Parameter("Show Level 2", GroupName = "Price Excursion Visuals", Description = "Whether level 2 lines are shown")]
	public bool ShowLevel2 { get; set; }
	
	[Parameter("Show Level 3", GroupName = "Price Excursion Visuals", Description = "Whether level 3 lines are shown")]
	public bool ShowLevel3 { get; set; }

	[Parameter("Level 1 Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 1 lines")]
	public Color Level1Color { get; set; } = DrawingColor.WhiteSmoke;

	[Parameter("Level 2 Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 2 lines")]
	public Color Level2Color { get; set; } = Color.Blue;

	[Parameter("Level 3 Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 3 lines")]
	public Color Level3Color { get; set; } = Color.Red;

	public void HidePriceExcursionParameters(Parameters parameters)
	{
		if (!ShowLevel1)
		{
			parameters.Remove(nameof(Level1Color));
		}

		if (!ShowLevel2)
		{
			parameters.Remove(nameof(Level2Color));
		}

		if (!ShowLevel3)
		{
			parameters.Remove(nameof(Level3Color));
		}
	}

	public void InitializePriceExcursions()
	{
		_priceExcursionAtr = new(256, MovingAverageType.Simple);

		PriceExcursion.LineStyle = LevelLineStyle;

		_priceExcursion = new(_priceExcursion.Result, 65);
	}

	public void RenderPriceExcursions(IDrawingContext drawingContext)
	{
		if (!EnableLevels || !ShowLevel1 && !ShowLevel2 && !ShowLevel3)
		{
			return;
		}

		var barIndexDelta = LevelPlotStyleValue switch
		{
			LevelPlotStyle.DynamicLines => 1,
			LevelPlotStyle.StraightLines => Chart.LastVisibleBarIndex - Chart.FirstVisibleBarIndex,
			_ => throw new UnreachableException(),
		};

		for (var barIndex = Chart.FirstVisibleBarIndex; barIndex < Chart.LastVisibleBarIndex;)
		{
			var nextBarIndex = barIndex + barIndexDelta;

			if (ShowLevel1)
			{
				DrawPriceExcursionLevel(drawingContext, barIndex, nextBarIndex, 1, Level1Color);
			}

			if (ShowLevel2)
			{
				DrawPriceExcursionLevel(drawingContext, barIndex, nextBarIndex, 2, Level2Color);
			}

			if (ShowLevel3)
			{
				DrawPriceExcursionLevel(drawingContext, barIndex, nextBarIndex, 3, Level3Color);
			}

			barIndex = nextBarIndex;
		}
	}

	private void DrawPriceExcursionLevel(IDrawingContext drawingContext
		, int startBarIndex, int endBarIndex, int levelMultiplier, Color levelColor)
    {
		ReadOnlySpan<int> levelSignums = [-1, 1];

		foreach (var levelSignum in levelSignums)
		{
			var startX = Chart.GetXCoordinateByBarIndex(startBarIndex);
			var startPrice = levelSignum * levelMultiplier * _priceExcursion[startBarIndex];
			var startY = ChartScale.GetYCoordinateByValue(startPrice);

			var endX = Chart.GetRightX();
			var endPrice = levelSignum * levelMultiplier * _priceExcursion[endBarIndex];
			var endPriceY = ChartScale.GetYCoordinateByValue(endPrice);

			drawingContext.DrawLine(startX, startY, endX, endPriceY, levelColor, LevelLineThickness, LevelLineStyle);
		}
    }

	public enum LevelPlotStyle
	{
		[DisplayName("Dynamic Lines")]
		DynamicLines,
		
		[DisplayName("Straight Lines")]
		StraightLines,
	}
}
