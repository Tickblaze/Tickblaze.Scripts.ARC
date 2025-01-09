﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class FairValueGaps : Indicator
{
	public FairValueGaps()
	{
		_menuViewModel = new(this);

		IsOverlay = true;
		ShortName = "TBC FVG";
		Name = "TB Core Fair Value Gaps";
	}

	[AllowNull]
	private Gaps _freshGaps;

	[AllowNull]
	private Gaps _testedGaps;

	[AllowNull]
	private Gaps _brokenGaps;

	[AllowNull]
	private AverageTrueRange _averageTrueRange;

	private readonly MenuViewModel _menuViewModel;

	[Parameter("Measurement")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("FVG Ticks")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = 0.01d, MaxValue = double.MaxValue, Step = 0.5d)]
	[Parameter("FVG ATR Multiple")]
	public double AtrMultiple { get; set; } = 0.5d;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Show Fresh FVGs", GroupName = "Visual Parameters")]
	public bool ShowFreshGaps { get; set; }

	[Parameter("Fresh FGV Color", GroupName = "Visual Parameters")]
	public Color FreshGapColor { get; set; } = Color.Orange.With(opacity: 0.5f);

	[Parameter("Show Tested FGVs", GroupName = "Visual Parameters")]
	public bool ShowTestedGaps { get; set; }

	[Parameter("Tested FGV Color", GroupName = "Visual Parameters")]
	public Color TestedGapColor { get; set; } = Color.Silver.With(opacity: 0.5f);

	[Parameter("Show Broken FGVs", GroupName = "Visual Parameters")]
	public bool ShowBrokenGaps { get; set; }

	[Parameter("Broken FGV Color", GroupName = "Visual Parameters")]
	public Color BrokenGapColor { get; set; } = Color.DimGray.With(opacity: 0.5f);

	[Parameter("Menu Header", GroupName = "Visual Parameters")]
	public string MenuHeader { get; set; } = "TBC FVG";

	public override object? CreateChartToolbarMenuItem()
    {
		var uri = new Uri("/Tickblaze.Scripts.Arc.Core;component/Indicators/FairValueGapsMenu.xaml", UriKind.Relative);

		var menu = Application.LoadComponent(uri);

		if (menu is FrameworkElement frameworkElement)
		{
			frameworkElement.DataContext = _menuViewModel;
		}

		return menu;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (GapMeasurementValue is GapMeasurement.Atr)
		{
			parameters.Remove(nameof(GapTickCount));
		}
		
		if (GapMeasurementValue is GapMeasurement.Tick)
		{
			parameters.RemoveRange([nameof(AtrPeriod), nameof(AtrMultiple)]);
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

	private ISeries<double> GetMinGapHeights()
	{
		if (GapMeasurementValue is GapMeasurement.Tick)
		{
			return Bars.Map(bar => GapTickCount * Symbol.TickSize);
		}
		else if (GapMeasurementValue is GapMeasurement.Atr)
		{
			var atr = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);

			return atr.Result.Map(atr => AtrMultiple * atr);
		}

		throw new UnreachableException();
	}

	protected override void Initialize()
	{
		var minGapHeights = GetMinGapHeights();

		_menuViewModel.Initialize();

		_freshGaps = new()
		{
			RenderParent = this,
			FillColor = FreshGapColor,
			MinHeights = minGapHeights,
		};

		_testedGaps = new()
		{
			RenderParent = this,
			FillColor = TestedGapColor,
			MinHeights = minGapHeights,
		};

		_brokenGaps = new()
		{
			RenderParent = this,
			FillColor = BrokenGapColor,
			MinHeights = minGapHeights,
		};
	}

	protected override void Calculate(int barIndex)
	{
		if (barIndex <= 1)
		{
			return;
		}

		CalculateFreshGaps(barIndex);
		CalculateTestedGaps(barIndex);
		CalculateBrokenGaps(barIndex);
	}

	private void CalculateFreshGaps(int barIndex)
	{
		var minGapHeight = _freshGaps.MinHeights[barIndex];

		Gap[] gaps =
		[
			new()
			{
				IsSupport = true,
				StartBarIndex = barIndex - 1,
				EndPrice = Bars.Low[barIndex],
				StartPrice = Bars.High[barIndex - 2],
			},
			new()
			{
				IsSupport = false,
				StartBarIndex = barIndex - 1,
				EndPrice = Bars.Low[barIndex - 2],
				StartPrice = Bars.High[barIndex],
			},
		];

		_freshGaps.Remove(barIndex - 1);

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
			var gap = _freshGaps.GetGapAt(gapIndex);

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
			var gap = _testedGaps.GetGapAt(gapIndex);

			gapIndex--;

			if (lastBar.Low < gap.StartPrice && gap.IsSupport
				|| lastBar.High > gap.EndPrice && gap.IsResistance)
			{
				gapIndex++;
				
				gap.EndBarIndex = index;

				_testedGaps.RemoveAt(gapIndex);

				_brokenGaps.AddOrUpdate(gap);
			}
		}
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		if (ShowFreshGaps)
		{
			_freshGaps.OnRender(drawingContext);
		}

		if (ShowTestedGaps)
		{
			_testedGaps.OnRender(drawingContext);
		}

		if (ShowBrokenGaps)
		{
			_brokenGaps.OnRender(drawingContext);
		}
	}

	public enum GapMeasurement
	{
		[DisplayName("ATR")]
		Atr,

		[DisplayName("Ticks")]
		Tick,
	}
}
