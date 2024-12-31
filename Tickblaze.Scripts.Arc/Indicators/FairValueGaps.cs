using System.Diagnostics;
using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;
using Point = Tickblaze.Scripts.Api.Models.Point;

namespace Tickblaze.Scripts.Arc;

public partial class FairValueGaps : Indicator
{
	public FairValueGaps()
	{
		//_menu = new(this);

		IsOverlay = true;
		ShortName = "TBC FVG";
		Name = "TB Core Fair Value Gaps";
	}

	//private readonly FairValueGapsMenu _menu;
	private AverageTrueRange _averageTrueRange;
	private readonly OrderedDictionary<int, Gap> _freshGaps = [];
	private readonly OrderedDictionary<int, Gap> _testedGaps = [];
	private readonly OrderedDictionary<int, Gap> _brokenGaps = [];
	
	[Parameter("Measurement", GroupName = "Parameters")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("FVG Ticks", GroupName = "Parameters")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = 0.01d, MaxValue = double.MaxValue, Step = 0.5d)]
	[Parameter("FVG ATR Multiple", GroupName = "Parameters")]
	public double AtrMultiple { get; set; } = 0.5d;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period", GroupName = "Parameters")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Show Fresh FVGs", GroupName = "Visuals")]
	public bool ShowFreshGaps { get; set; } = true;

	[Parameter("Fresh FGV Color", GroupName = "Visuals")]
	public Color FreshGapColor { get; set; } = Color.New(Color.Orange, 0.5f);

	[Parameter("Show Tested FGVs", GroupName = "Visuals")]
	public bool ShowTestedGaps { get; set; } = true;

	[Parameter("Tested FGV Color", GroupName = "Visuals")]
	public Color TestedGapColor { get; set; } = Color.New(Color.Silver, 0.5f);

	[Parameter("Show Broken FGVs", GroupName = "Visuals")]
	public bool ShowBrokenGaps { get; set; } = true;

	[Parameter("Broken FGV Color", GroupName = "Visuals")]
	public Color BrokenGapColor { get; set; } = Color.New(Color.DimGray, 0.5f);

	[Parameter("Button Text", GroupName = "Visuals")]
	public string ButtonText { get; set; } = "TrapFinder";

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

		if (!ShowFreshGaps)
		{
			parameters.Remove(nameof(FreshGapColor));
		}

		if (!ShowTestedGaps)
		{
			parameters.Remove(nameof(TestedGapColor));
		}

		if (!ShowBrokenGaps)
		{
			parameters.Remove(nameof(BrokenGapColor));
		}

		return parameters;
	}

	//public override object? CreateChartToolbarMenuItem()
	//{
	//    return _menu;
	//}
	
	protected override void Initialize()
	{
		_averageTrueRange = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
	}

	protected override void Calculate(int index)
	{
		if (index <= 1)
		{
			return;
		}

		CalculateFreshGaps(index);
		CalculateTestedGaps(index);
		CalculateBrokenGaps(index);
	}

	private void CalculateFreshGaps(int index)
	{
		var minGapHeight = GapMeasurementValue switch
		{
			GapMeasurement.Atr => AtrMultiple * _averageTrueRange![index],
			GapMeasurement.Tick => GapTickCount * Symbol.TickSize,
			_ => throw new UnreachableException()
		};

		Gap[] gaps =
		[
			new()
			{
				IsSupport = true,
				StartBarIndex = index - 1,
				EndPrice = Bars.Low[index],
				StartPrice = Bars.High[index - 2],
			},
			new()
			{
				IsSupport = false,
				StartBarIndex = index - 1,
				EndPrice = Bars.Low[index - 2],
				StartPrice = Bars.High[index],
			},
		];

		_freshGaps.Remove(index - 1);

		foreach (var gap in gaps)
		{
			if (gap.EndPrice - gap.StartPrice > minGapHeight)
			{
				_freshGaps.Add(gap.StartBarIndex, gap);
			}
		}
	}

	private void CalculateTestedGaps(int index)
	{
		var lastBar = Bars[index]!;
		
		for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _freshGaps.GetValueAt(gapIndex);

			if (index - gap.StartBarIndex <= 1)
			{
				continue;
			}

			if (lastBar.Low < gap.EndPrice && gap.IsSupport
				|| lastBar.High > gap.StartPrice && gap.IsResistance)
			{
				gap.EndBarIndex = index;

				_freshGaps.RemoveAt(gapIndex);

				_testedGaps.Add(gap.StartBarIndex, gap);
			}
		}
	}

	private void CalculateBrokenGaps(int index)
	{
		var lastBar = Bars[index]!;

		for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _testedGaps.GetValueAt(gapIndex);

			gapIndex--;

			if (lastBar.Low < gap.StartPrice && gap.IsSupport
				|| lastBar.High > gap.EndPrice && gap.IsResistance)
			{
				gapIndex++;
				
				gap.EndBarIndex = index;

				_testedGaps.RemoveAt(gapIndex);

				_brokenGaps.Add(gap.StartBarIndex, gap);
			}
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		if (ShowFreshGaps)
		{
			RenderGaps(context, FreshGapColor, _freshGaps.Values);
		}

		if (ShowTestedGaps)
		{
			RenderGaps(context, TestedGapColor, _testedGaps.Values);
		}

		if (ShowBrokenGaps)
		{
			RenderGaps(context, BrokenGapColor, _brokenGaps.Values);
		}
	}

	private void RenderGaps(IDrawingContext drawingContext, Color fillColor, IEnumerable<Gap> gaps)
	{
		foreach (var gap in gaps)
		{
			var fromIndex = gap.StartBarIndex;
			var toIndex = gap.EndBarIndex ?? Math.Max(gap.StartBarIndex, Chart.LastVisibleBarIndex);

			if (toIndex - fromIndex >= 1
				&& AreIntervalsIntersect(gap.StartPrice, gap.EndPrice, ChartScale.MinPrice, ChartScale.MaxPrice)
				&& AreIntervalsIntersect(fromIndex, toIndex, Chart.FirstVisibleBarIndex, Chart.LastVisibleBarIndex))
			{
				var fromX = Chart.GetXCoordinateByBarIndex(fromIndex);
				var fromY = ChartScale.GetYCoordinateByValue(gap.EndPrice);

				var toX = Chart.GetXCoordinateByBarIndex(toIndex);
				var toY = ChartScale.GetYCoordinateByValue(gap.StartPrice);
				
				var topLeftPoint = new Point(fromX, fromY);
				var bottomRightPoint = new Point(toX, toY);

				drawingContext.DrawRectangle(topLeftPoint, bottomRightPoint, fillColor);
			}
		}
	}

	private static bool AreIntervalsIntersect(double firstIntervalStart,
		double firstIntervalEnd, double secondIntervalStart, double secondIntervalEnd)
	{
		return firstIntervalStart <= secondIntervalEnd && secondIntervalStart <= firstIntervalEnd;
	}
}
