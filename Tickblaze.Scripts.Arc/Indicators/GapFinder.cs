using System.Diagnostics;
using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public partial class GapFinder : Indicator
{
	public GapFinder()
	{
		IsOverlay = true;
		ShortName = "TBC GF";
		Name = "TB Core Gap Finder";
	}

	private AverageTrueRange _averageTrueRange = new();

	private readonly DrawingPartDictionary<int, Gap> _freshGaps = [];
	private readonly DrawingPartDictionary<int, Gap> _testedGaps = [];
	private readonly DrawingPartDictionary<int, Gap> _brokenGaps = [];

	[Parameter("Measurement")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Ticks")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Points")]
	public int GapPointCount { get; set; } = 5;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Pips")]
	public int GapPipCount { get; set; } = 20;

	[NumericRange(MinValue = 0.01, MaxValue = double.MaxValue, Step = 0.5d)]
	[Parameter("Gap ATR Multiple")]
	public double AtrMultiple { get; set; } = 0.5;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Restrict to New Session")]
	public bool IsRestrictedToNewSession { get; set; }

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

	[Parameter("Settings Header", GroupName = "Visuals")]
	public string SettingsHeader { get; set; } = "GapFinder";

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
		if (IsRestrictedToNewSession && !this.IsNewSession(index))
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
				_freshGaps.AddOrUpdate(gap);
			}
		}
	}

	private void CalculateTestedGaps(int index)
	{
		var lastBar = Bars[index]!;
		
		for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _freshGaps.GetDrawingPartAt(gapIndex);

			if (index - gap.StartBarIndex <= 1)
			{
				continue;
			}

			if (lastBar.Low < gap.EndPrice && gap.IsSupport
				|| lastBar.High > gap.StartPrice && gap.IsResistance)
			{
				gap.EndBarIndex = index;

				_freshGaps.RemoveAt(gapIndex);

				_testedGaps.AddOrUpdate(gap);
			}
		}
	}

	private void CalculateBrokenGaps(int index)
	{
		var lastBar = Bars[index]!;

		for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _testedGaps.GetDrawingPartAt(gapIndex);

			if (lastBar.Low < gap.StartPrice && gap.IsSupport
				|| lastBar.High > gap.EndPrice && gap.IsResistance)
			{
				gap.EndBarIndex = index;

				_testedGaps.RemoveAt(gapIndex);

				_brokenGaps.AddOrUpdate(gap);
			}
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		if (ShowFreshGaps)
		{
			RenderGaps(context, FreshGapColor, _freshGaps);
		}

		if (ShowTestedGaps)
		{
			RenderGaps(context, TestedGapColor, _testedGaps);
		}

		if (ShowBrokenGaps)
		{
			RenderGaps(context, BrokenGapColor, _brokenGaps);
		}
	}

	private void RenderGaps(IDrawingContext drawingContext, Color fillColor, DrawingPartDictionary<int, Gap> gaps)
	{
		var visibleBoundary = this.GetVisibleBoundary();

		var chartRightX = Chart.GetRightX();
		var visibleGaps = gaps.GetVisibleDrawingParts(visibleBoundary);

		foreach (var gap in visibleGaps)
        {
            if (gap.IsEmpty)
            {
                continue;
            }

            var gapStartX = Chart.GetXCoordinateByBarIndex(gap.StartBarIndex);
            var gapStartY = ChartScale.GetYCoordinateByValue(gap.StartPrice);

            var gapEndX = gap.EndBarIndex is null
				? chartRightX : Chart.GetXCoordinateByBarIndex(gap.EndBarIndex.Value);
            var gapEndY = ChartScale.GetYCoordinateByValue(gap.EndPrice);

            drawingContext.DrawRectangle(gapStartX, gapStartY, gapEndX, gapEndY, fillColor);
        }
    }
}
