using System.Diagnostics;
using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;
using Point = Tickblaze.Scripts.Api.Models.Point;

namespace Tickblaze.Scripts.Arc;

public partial class GapFinder : Indicator
{
	public GapFinder()
	{
		IsOverlay = true;
		ShortName = "TBC GF";
		Name = "TB Core Gap Finder";
	}

	private AverageTrueRange _averageTrueRange;

	private readonly OrderedDictionary<int, Gap> _freshGaps = [];
	private readonly OrderedDictionary<int, Gap> _testedGaps = [];
	private readonly OrderedDictionary<int, Gap> _brokenGaps = [];

	[Parameter("Measurement", GroupName = "Parameters")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap Ticks", GroupName = "Parameters")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap Pts", GroupName = "Parameters")]
	public int GapPointCount { get; set; } = 5;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap Pts", GroupName = "Parameters")]
	public int GapPipCount { get; set; } = 20;

	[NumericRange(MinValue = 0.01, MaxValue = double.MaxValue, Step = 0.5d)]
	[Parameter("Gap ATR Multiple", GroupName = "Parameters")]
	public double AtrMultiple { get; set; } = 0.5;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period", GroupName = "Parameters")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Restrict to New Session", GroupName = "Parameters")]
	public bool IsRestrictedToNewSessions { get; set; }

	[Parameter("Show Fresh Gaps", GroupName = "Visuals")]
	public bool ShowFreshGaps { get; set; } = true;

	[Parameter("Fresh Gap Color", GroupName = "Visuals")]
	public Color FreshGapColor { get; set; } = Color.New(Color.Orange, 0.5f);

	[Parameter("Show Tested Gaps", GroupName = "Visuals")]
	public bool ShowTestedGaps { get; set; } = true;

	[Parameter("Tested Gap Color", GroupName = "Visuals")]
	public Color TestedGapColor { get; set; } = Color.New(Color.Silver, 0.5f);

	[Parameter("Show Broken Gaps", GroupName = "Visuals")]
	public bool ShowBrokenGaps { get; set; } = true;

	[Parameter("Broken Gap Color", GroupName = "Visuals")]
	public Color BrokenGapColor { get; set; } = Color.New(Color.DimGray, 0.5f);

	[Parameter("Button Text", GroupName = "Visuals")]
	public string ButtonText { get; set; } = "GapFinder";

	private bool IsNewSessionBar(int index)
	{
		var exchangeCalendar = Bars.Symbol.ExchangeCalendar;

		var currentBarTimeUtc = Bars.Time[index];
		var previousBarTimeUtc = Bars.Time[index - 1];

		var currentSession = exchangeCalendar.GetSession(currentBarTimeUtc);
		var previousSession = exchangeCalendar.GetSession(previousBarTimeUtc);

		return currentSession is not null
			&& previousSession is not null
			&& DateTime.Equals(currentSession.StartUtcDateTime, previousSession.StartUtcDateTime);
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		List<string> gapSizePropertyNames =
		[
			nameof(GapTickCount),
			nameof(GapPointCount),
			nameof(GapPipCount),
			nameof(AtrMultiple),
			nameof(AtrPeriod),
		];

		var _ = GapMeasurementValue switch
		{
			GapMeasurement.Tick => gapSizePropertyNames.Remove(nameof(GapTickCount)),
			GapMeasurement.Point => gapSizePropertyNames.Remove(nameof(GapPointCount)),
			GapMeasurement.Pip => gapSizePropertyNames.Remove(nameof(GapPipCount)),
			GapMeasurement.Atr => gapSizePropertyNames.Remove(nameof(AtrMultiple))
				& gapSizePropertyNames.Remove(nameof(AtrPeriod)),
			_ => throw new UnreachableException()
		};

		gapSizePropertyNames.ForEach(propertyName => parameters.Remove(propertyName));

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
		if (IsRestrictedToNewSessions && !IsNewSessionBar(index))
		{
			return;
		}

		var tickSize = Symbol.TickSize;
		var minGapHeight = GapMeasurementValue switch
		{
			GapMeasurement.Point => GapPointCount,
			GapMeasurement.Pip => 10 * GapPipCount * tickSize,
			GapMeasurement.Tick => GapTickCount * tickSize,
			GapMeasurement.Atr => AtrMultiple * _averageTrueRange[index],
			_ => throw new UnreachableException()
		};

		Gap[] gaps =
		[
			new()
			{
				IsSupport = true,
				StartBarIndex = index - 1,
				EndPrice = Bars.Open[index],
				StartPrice = Bars.Close[index - 1],
			},
			new()
			{
				IsSupport = false,
				StartBarIndex = index - 1,
				EndPrice = Bars.Close[index - 1],
				StartPrice = Bars.Open[index],
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

			if (lastBar.Low < gap.StartPrice && gap.IsSupport
				|| lastBar.High > gap.EndPrice && gap.IsResistance)
			{
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

				var topLeft = new Point(fromX, fromY);
				var bottomRight = new Point(toX, toY);

				drawingContext.DrawRectangle(topLeft, bottomRight, fillColor);
			}
		}
	}

	private static bool AreIntervalsIntersect(double firstIntervalStart,
		double firstIntervalEnd, double secondIntervalStart, double secondIntervalEnd)
	{
		return firstIntervalStart <= secondIntervalEnd && secondIntervalStart <= firstIntervalEnd;
	}
}
