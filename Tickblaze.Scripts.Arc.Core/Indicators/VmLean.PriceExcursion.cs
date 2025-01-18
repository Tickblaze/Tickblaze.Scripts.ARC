using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	private int _currentIndex;

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
	public bool ShowLevel1 { get; set; }

	[Parameter("Show Level 2 Lines", GroupName = "Price Excursion Visuals", Description = "Whether level 2 lines are shown")]
	public bool ShowLevel2 { get; set; }
	
	[Parameter("Show Level 3 Lines", GroupName = "Price Excursion Visuals", Description = "Whether level 3 lines are shown")]
	public bool ShowLevel3 { get; set; }

	[Parameter("Level 1 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 1 lines")]
	public Color Level1Color { get; set; } = DrawingColor.WhiteSmoke;

	[Parameter("Level 2 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 2 lines")]
	public Color Level2Color { get; set; } = Color.Blue;

	[Parameter("Level 3 Line Color", GroupName = "Price Excursion Visuals", Description = "Color of the level 3 lines")]
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
		_currentIndex = 0;

		_priceExcursionAtr = new(256, MovingAverageType.Simple);

		_priceExcursion = new(_priceExcursionAtr.Result, 65);
	}

	private void CalculatePriceExcursions(int barIndex)
	{
		if (_currentIndex.Equals(barIndex))
		{
			return;
		}

		_currentIndex = barIndex;

		// Todo: fix wrong calculations [calculating on bar open, not close];
		_priceExcursionAtr.Calculate();

		_priceExcursion.Calculate();
	}

	public void RenderPriceExcursions(IDrawingContext drawingContext)
	{
		if (!EnableLevels || !ShowLevel1 && !ShowLevel2 && !ShowLevel3)
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

		if (ShowLevel1)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 1, Level1Color);
		}

		if (ShowLevel2)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 2, Level2Color);
		}

		if (ShowLevel3)
		{
			DrawPriceExcursionLevel(drawingContext, barIndexes, 3, Level3Color);
		}
	}

	private void DrawPriceExcursionLevel(IDrawingContext drawingContext
		, IEnumerable<int> barIndexes, int levelMultiplier, Color levelLineColor)
    {
		ReadOnlySpan<int> levelSignums = [-1, 1];

		foreach (var levelSignum in levelSignums)
		{
			var levelApiPoints = barIndexes
				.Select(PriceExcursion.GetPoint)
				.Select(point => point with
				{
					Price = levelSignum * levelMultiplier * point.Price
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
