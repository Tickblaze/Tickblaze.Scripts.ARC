using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	private int _priceExcursionBarIndex;

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

	[Parameter("Show Level 1 Lines", GroupName = "Price Excursion Visuals", Description = "Whether level 1 lines are shown")]
	public bool ShowLevel1Lines { get; set; }

	[Parameter("Show Level 2 Lines", GroupName = "Price Excursion Visuals", Description = "Whether level 2 lines are shown")]
	public bool ShowLevel2Lines { get; set; }
	
	[Parameter("Show Level 3 Lines", GroupName = "Price Excursion Visuals", Description = "Whether level 3 lines are shown")]
	public bool ShowLevel3Lines { get; set; }

	[Parameter("Level 1 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 1 lines")]
	public Color Level1LineColor { get; set; } = DrawingColor.WhiteSmoke;

	[Parameter("Level 2 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 2 lines")]
	public Color Level2LineColor { get; set; } = Color.Blue;

	[Parameter("Level 3 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 3 lines")]
	public Color Level3LineColor { get; set; } = Color.Red;

	public void HidePriceExcursionParameters(Parameters parameters)
	{
		if (!ShowLevel1Lines)
		{
			parameters.Remove(nameof(Level1LineColor));
		}

		if (!ShowLevel2Lines)
		{
			parameters.Remove(nameof(Level2LineColor));
		}

		if (!ShowLevel3Lines)
		{
			parameters.Remove(nameof(Level3LineColor));
		}
	}

	public void InitializePriceExcursions()
	{
		_priceExcursionAtr = new(256, MovingAverageType.Simple);

		_priceExcursion = new(_priceExcursionAtr.Result, 65);
	}

	private void CalculatePriceExcursions(int barIndex)
	{
		_priceExcursionAtr.Calculate();

		_priceExcursion.Calculate();
	}

	public void RenderPriceExcursions(IDrawingContext drawingContext)
	{
		if (!EnableLevels || !ShowLevel1Lines && !ShowLevel2Lines && !ShowLevel3Lines)
		{
			return;
		}

		var lastBarIndex = Chart.LastVisibleBarIndex;
		var firstBarIndex = Chart.FirstVisibleBarIndex;

		var barIndexes = LevelPlotStyleValue switch
		{
			LevelPlotStyle.StraightLines => [firstBarIndex, lastBarIndex],
			LevelPlotStyle.DynamicLines => Enumerable.Range(firstBarIndex, lastBarIndex - firstBarIndex),
			_ => throw new UnreachableException(),
		};

		if (ShowLevel1Lines)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 1, Level1LineColor);
		}

		if (ShowLevel2Lines)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 2, Level2LineColor);
		}

		if (ShowLevel3Lines)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 3, Level3LineColor);
		}
	}

	private void DrawPriceExcursionLevel(IDrawingContext drawingContext
		, IEnumerable<int> barIndexes, int levelMultiplier, Color levelLineColor)
    {
		ReadOnlySpan<int> levelSignums = [-1, 1];

		foreach (var levelSignum in levelSignums)
		{
			var levelApiPoints = barIndexes
				.Select(barIndex => new Point
				{
					BarIndex = barIndex,
					Price = levelSignum * levelMultiplier * PriceExcursion[barIndex],
				})
				.Select(this.GetApiPoint);

			drawingContext.DrawPolygon(levelApiPoints, default, levelLineColor, LevelLineThickness, LevelLineStyle);
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
