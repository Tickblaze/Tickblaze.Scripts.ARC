using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class GapFinder : Indicator
{
	public GapFinder()
	{
		_menuViewModel = new(this);

		IsOverlay = true;
		ShortName = "GF";
		Name = "Gap Finder";
	}

	private readonly MenuViewModel _menuViewModel;

	[AllowNull]
	private Gaps _freshGaps;

	[AllowNull]
	private Gaps _testedGaps;

	[AllowNull]
	private Gaps _brokenGaps;

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

	[Parameter("Show Fresh Gaps", GroupName = "Visual Parameters")]
	public bool ShowFreshGaps { get; set; } = true;

	[Parameter("Fresh Gap Color", GroupName = "Visual Parameters")]
	public Color FreshGapColor { get; set; } = Color.Orange.With(opacity: 0.5f);

	[Parameter("Show Tested Gaps", GroupName = "Visual Parameters")]
	public bool ShowTestedGaps { get; set; } = true;

	[Parameter("Tested Gap Color", GroupName = "Visual Parameters")]
	public Color TestedGapColor { get; set; } = Color.Silver.With(opacity: 0.5f);

	[Parameter("Show Broken Gaps", GroupName = "Visual Parameters")]
	public bool ShowBrokenGaps { get; set; } = true;

	[Parameter("Broken Gap Color", GroupName = "Visual Parameters")]
	public Color BrokenGapColor { get; set; } = Color.DimGray.With(opacity: 0.5f);

	[Parameter("Settings Header", GroupName = "Visual Parameters")]
	public string SettingsHeader { get; set; } = "Gap Finder";

	public override object? CreateChartToolbarMenuItem()
	{
		var uri = new Uri("/Tickblaze.Scripts.Arc.Core;component/Indicators/GapFinder.Menu.xaml", UriKind.Relative);

		var menuObject = Application.LoadComponent(uri);

		if (menuObject is Menu menu)
		{
			menu.DataContext = _menuViewModel;
		}

		return menuObject;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		List<string> propertyNames =
		[
			nameof(GapTickCount),
			nameof(GapPointCount),
			nameof(GapPipCount),
			nameof(AtrMultiple),
			nameof(AtrPeriod),
		];

		var _ = GapMeasurementValue switch
		{
			GapMeasurement.Tick => propertyNames.Remove(nameof(GapTickCount)),
			GapMeasurement.Point => propertyNames.Remove(nameof(GapPointCount)),
			GapMeasurement.Pip => propertyNames.Remove(nameof(GapPipCount)),
			GapMeasurement.Atr => propertyNames.Remove(nameof(AtrMultiple))
				& propertyNames.Remove(nameof(AtrPeriod)),
			_ => throw new UnreachableException()
		};

		propertyNames.ForEach(propertyName => parameters.Remove(propertyName));

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
		var tickSize = Symbol.TickSize;
		
		return GapMeasurementValue switch
		{
			GapMeasurement.Point => Bars.Map(bar => 1.0 * GapPointCount),
			GapMeasurement.Pip => Bars.Map(bar => 10.0 * GapPipCount * tickSize),
			GapMeasurement.Tick => Bars.Map(bar => GapTickCount * tickSize),
			GapMeasurement.Atr => GetAtrMinGapHeights(),
			_ => throw new UnreachableException()
		};
	}

	private ISeries<double> GetAtrMinGapHeights()
	{
		var atr = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);

		return atr.Result.Map(atr => AtrMultiple * atr);
	}

	protected override void Initialize()
	{
		_menuViewModel.Initialize();

		var minGapHeights = GetMinGapHeights();

		_freshGaps = new()
		{
			RenderTarget = this,
			FillColor = FreshGapColor,
			MinHeights = minGapHeights,
		};

		_testedGaps = new()
		{
			RenderTarget = this,
			FillColor = TestedGapColor,
			MinHeights = minGapHeights,
		};

		_brokenGaps = new()
		{
			RenderTarget = this,
			FillColor = BrokenGapColor,
			MinHeights = minGapHeights,
		};
	}

	protected override void Calculate(int barIndex)
	{
		if (barIndex is 0)
		{
			return;
		}

		CalculateFreshGaps(barIndex);
		CalculateTestedGaps(barIndex);
		CalculateBrokenGaps(barIndex);
	}

	private void CalculateFreshGaps(int barIndex)
	{
		if (IsRestrictedToNewSession && !this.IsNewSession(barIndex))
		{
			return;
		}

		var minGapHeight = _freshGaps.MinHeights[barIndex - 1];

		Gap[] gaps =
		[
			new()
			{
				IsSupport = true,
				StartBarIndex = barIndex - 1,
				EndPrice = Bars.Open[barIndex],
				StartPrice = Bars.Close[barIndex - 1],
			},
			new()
			{
				IsSupport = false,
				StartBarIndex = barIndex - 1,
				EndPrice = Bars.Close[barIndex - 1],
				StartPrice = Bars.Open[barIndex],
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

	private void CalculateTestedGaps(int barIndex)
	{
		var lastBar = Bars[barIndex]!;
		
		for (var gapIndex = _freshGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _freshGaps.GetGapAt(gapIndex);

			if (barIndex - gap.StartBarIndex <= 1)
			{
				continue;
			}

			if (lastBar.Low < gap.EndPrice && gap.IsSupport
				|| lastBar.High > gap.StartPrice && gap.IsResistance)
			{
				gap.EndBarIndex = barIndex;

				_freshGaps.RemoveAt(gapIndex);

				_testedGaps.AddOrUpdate(gap);
			}
		}
	}

	private void CalculateBrokenGaps(int barIndex)
	{
		var lastBar = Bars[barIndex]!;

		for (var gapIndex = _testedGaps.Count - 1; gapIndex >= 0; gapIndex--)
		{
			var gap = _testedGaps.GetGapAt(gapIndex);

			if (lastBar.Low < gap.StartPrice && gap.IsSupport
				|| lastBar.High > gap.EndPrice && gap.IsResistance)
			{
				gap.EndBarIndex = barIndex;

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
		[DisplayName("Tick")]
		Tick,

		[DisplayName("Point")]
		Point,

		[DisplayName("Pip")]
		Pip,

		[DisplayName("Atr")]
		Atr,
	}
}
