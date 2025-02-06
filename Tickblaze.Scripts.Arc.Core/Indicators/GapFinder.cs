using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

	[AllowNull]
	private Gaps _freshGaps;

	[AllowNull]
	private Gaps _testedGaps;

	[AllowNull]
	private Gaps _brokenGaps;
	
	private readonly MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Tickblaze.Scripts.Arc.Core.Indicators.GapFinder.Menu.xaml";

	[Parameter("Gap Measurement Type", Description = "Type of the gap measurement")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Ticks", Description = "Minimum price delta in ticks needed to create a gap")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Points", Description = "Minimum price delta in points needed to create a gap")]
	public int GapPointCount { get; set; } = 5;

	[NumericRange(MinValue = 1)]
	[Parameter("Gap in Pips", Description = "Minimum price delta in pips = 10 * ticks needed to create a gap")]
	public int GapPipCount { get; set; } = 20;

	[NumericRange(MinValue = 0.01, MaxValue = double.MaxValue, Step = 0.5d)]
	[Parameter("Gap in ATR Multiples", Description = "Minimum price delta in ATR multiples needed to create a gap")]
	public double AtrMultiple { get; set; } = 0.5;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period", Description = "Period of the ATR")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Restrict to New Session", Description = "Whether gap is calculated only on session change")]
	public bool IsRestrictedToNewSession { get; set; }

	[Parameter("Menu Header", GroupName = "Visuals", Description = "Header of the menu")]
	public string MenuHeader { get; set; } = "Gap Finder";

	[Parameter("Show Fresh Gaps", GroupName = "Visuals", Description = "Whether fresh gaps are shown")]
	public bool ShowFreshGaps { get; set; } = true;

	[Parameter("Show Tested Gaps", GroupName = "Visuals", Description = "Whether tested gaps are shown")]
	public bool ShowTestedGaps { get; set; } = true;

	[Parameter("Show Broken Gaps", GroupName = "Visuals", Description = "Whether broken gaps are shown")]
	public bool ShowBrokenGaps { get; set; } = true;
	
	[Parameter("Fresh Gap Color", GroupName = "Visuals", Description = "Color of the fresh gap")]
	public Color FreshGapColor { get; set; } = Color.Orange.With(opacity: 0.5f);
	
	[Parameter("Tested Gap Color", GroupName = "Visuals", Description = "Color of the tested gap")]
	public Color TestedGapColor { get; set; } = Color.Silver.With(opacity: 0.5f);

	[Parameter("Broken Gap Color", GroupName = "Visuals", Description = "Color of the broken gap")]
	public Color BrokenGapColor { get; set; } = Color.DimGray.With(opacity: 0.5f);

	public override object? CreateChartToolbarMenuItem()
	{
		var menu = ResourceDictionaries.LoadResource<Menu>(_menuResourceName);

		menu.DataContext = _menuViewModel;

		return menu;
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

		_ = GapMeasurementValue switch
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
            GapMeasurement.Point => Bars.Select(bar => 1.0 * GapPointCount),
            GapMeasurement.Pip => Bars.Select(bar => 10.0 * GapPipCount * tickSize),
            GapMeasurement.Tick => Bars.Select(bar => GapTickCount * tickSize),
            GapMeasurement.Atr => GetAtrMinGapHeights(),
			_ => throw new UnreachableException()
		};
	}

	private ISeries<double> GetAtrMinGapHeights()
	{
		var atr = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);

		return atr.Result.Select(atr => AtrMultiple * atr);
	}

	protected override void Initialize()
	{
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

		_menuViewModel.Initialize();
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
