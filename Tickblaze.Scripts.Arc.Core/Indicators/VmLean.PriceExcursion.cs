using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[AllowNull]
	private AverageTrueRange _priceExcursionAtr;

	[AllowNull]
	private SimpleMovingAverage _priceExcursion;

	[Parameter("Level Plot Style", GroupName = "Price Excursion Parameters", Description = "Plot style of level lines")]
	public LevelPlotStyle LevelPlotStyleValue { get; set; } = LevelPlotStyle.StraightLines;

	[Parameter("Level Line Style", GroupName = "Price Excursion Parameters", Description = "Style of level lines")]
	public LineStyle LevelLineStyle { get; set; } = LineStyle.Solid;

	[NumericRange(MinValue = 1)]
	[Parameter("Level Line Thickness", GroupName = "Price Excursion Parameters", Description = "Thickness of level lines")]
	public int LevelLineThickness { get; set; } = 3;

	[Parameter("Show Level 1", GroupName = "Price Excursion Parameters", Description = "Whether level 1 is shown")]
	public bool ShowLevel1 { get; set; }

	[Parameter("Level 1 Color", GroupName = "Price Excursion Parameters")]
	public Color Level1Color { get; set; } = DrawingColor.WhiteSmoke;

	[Parameter("Show Level 2", GroupName = "Price Excursion Parameters", Description = "Whether level 2 is shown")]
	public bool ShowLevel2 { get; set; }

	[Parameter("Level 2 Color", GroupName = "Price Excursion Parameters")]
	public Color Level2Color { get; set; } = Color.Blue;

	[Parameter("Show Level 3", GroupName = "Price Excursion Parameters", Description = "Whether level 3 is shown")]
	public bool ShowLevel3 { get; set; }
	
	[Parameter("Level 3 Color", GroupName = "Price Excursion Parameters")]
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

		_priceExcursion = new(_priceExcursion.Result, 65);
	}

	public void RenderPriceExcursions(IDrawingContext drawingContext)
	{
		if (!ShowLevel1 && !ShowLevel2 && !ShowLevel3)
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

			drawingContext.DrawLine(startX, startY, endX, endPriceY, levelColor, LevelLineThickness);
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
