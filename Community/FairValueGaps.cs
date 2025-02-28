using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Controls;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Community;

public partial class FairValueGaps : Indicator
{
	public FairValueGaps()
	{
		var assemblyName = Assembly
			.GetExecutingAssembly()
			.GetName();

		_menuViewModel = new(this);
		
		_menuResourceName = assemblyName.Name + ".FairValueGaps.Menu.xaml";

		IsOverlay = true;
		
		ShortName = "FVG";
		
		Name = "Fair Value Gaps";
	}

	[AllowNull]
	private Gaps _freshGaps;

	[AllowNull]
	private Gaps _testedGaps;

	[AllowNull]
	private Gaps _brokenGaps;

	private readonly string _menuResourceName;

	private readonly MenuViewModel _menuViewModel;
	
	[Parameter("Gap Measurement Type", Description = "Type of the gap measurement")]
	public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

	[NumericRange(MinValue = 1)]
	[Parameter("FVG in Ticks", Description = "Minimum price delta in ticks needed to create a gap")]
	public int GapTickCount { get; set; } = 8;

	[NumericRange(MinValue = DoublePositiveMin, MaxValue = double.MaxValue, Step = DoubleStep)]
	[Parameter("FVG in ATR Multiples", Description = "Minimum price delta in ATR multiples needed to create a gap")]
	public double AtrMultiple { get; set; } = 0.5d;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period", Description = "Period of the ATR")]
	public int AtrPeriod { get; set; } = 14;

	[Parameter("Menu Header", GroupName = "Visuals", Description = "Header of the menu")]
	public string MenuHeader { get; set; } = "Fair Value Gaps";

	[Parameter("Show Fresh FVGs", GroupName = "Visuals", Description = "Whether fresh FVGs are shown")]
	public bool ShowFreshGaps { get; set; } = true;

	[Parameter("Show Tested FGVs", GroupName = "Visuals", Description = "Whether tested FVGs are shown")]
	public bool ShowTestedGaps { get; set; } = true;

	[Parameter("Show Broken FGVs", GroupName = "Visuals", Description = "Whether broken FVGs are shown")]
	public bool ShowBrokenGaps { get; set; } = true;

	[Parameter("Fresh FGV Color", GroupName = "Visuals", Description = "Color of the fresh FVG")]
	public Color FreshGapColor { get; set; } = Color.Orange.With(opacity: 0.5f);

	[Parameter("Tested FGV Color", GroupName = "Visuals", Description = "Color of the tested FVG")]
	public Color TestedGapColor { get; set; } = Color.Silver.With(opacity: 0.5f);

	[Parameter("Broken FGV Color", GroupName = "Visuals", Description = "Color of the broken FVG")]
	public Color BrokenGapColor { get; set; } = Color.DimGray.With(opacity: 0.5f);

	public override object? CreateChartToolbarMenuItem()
    {
		var menu = ResourceDictionaries.LoadResource<Menu>(_menuResourceName);

		menu.DataContext = _menuViewModel;

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
			return Bars.Select(bar => GapTickCount * Symbol.TickSize);
		}
		else if (GapMeasurementValue is GapMeasurement.Atr)
		{
			var atr = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);

			return atr.Result.Select(atr => AtrMultiple * atr);
		}

		throw new UnreachableException();
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
		if (barIndex <= 1)
		{
			return;
		}

		if (Bars[barIndex].Time.Day == 21)
		{

		}

		CalculateFreshGaps(barIndex);

		CalculateTestedGaps(barIndex);
		
		CalculateBrokenGaps(barIndex);
	}

	private void CalculateFreshGaps(int barIndex)
	{
		var minGapHeight = _freshGaps.MinHeights[barIndex - 1];

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
		[DisplayName("ATR")]
		Atr,

		[DisplayName("Ticks")]
		Tick,
	}
}
