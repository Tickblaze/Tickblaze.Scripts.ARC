using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace Tickblaze.Community;

public partial class VmLean : Indicator
{
	public VmLean()
	{
		_menuViewModel = new(this);

		Name = "VM Lean";

		ShortName = "VML";
	}

	private readonly Lock _lock = new();

	[AllowNull]
	private VmLeanCore _vmLeanCore;

	[AllowNull]
	private readonly MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Community.Indicators.VmLean.Menu.xaml";

	[Parameter("Menu Header", Description = "Header of the menu")]
	public string MenuHeader { get; set; } = "VM Lean";

	[Plot("Zero Line")]
	public PlotSeries ZeroLine { get; set; } = new(Color.Black, LineStyle.Solid, 2);

	public override object? CreateChartToolbarMenuItem()
	{
		var menu = ResourceDictionaries.LoadResource<Menu>(_menuResourceName);

		menu.DataContext = _menuViewModel;

		return menu;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		HidePriceExcursionParameters(parameters);

		return parameters;
	}

	protected override void Initialize()
	{
		using var lockScope = _lock.EnterScope();

		_vmLeanCore = new VmLeanCore
		{
			Bars = Bars,
			RenderTarget = this,

			BandPeriod = BandPeriod,
			BandMultiplier = BandMultiplier,
			MacdSlowPeriod = MacdSlowPeriod,
			MacdFastPeriod = MacdFastPeriod,
		};

		_vmLeanCore.Reinitialize(this);

		InitializeSwings(false);

		InitializeMacdBb();

		InitializeFlooding(false);

		InitializePriceExcursions();

		_menuViewModel.Initialize();
	}

	protected override void Calculate(int barIndex)
	{
		using var lockScope = _lock.EnterScope();

		ZeroLine[barIndex] = 0.0;

		_vmLeanCore.Calculate();

		CalculateHistogram(barIndex);

		CalculateMacdBb(barIndex);

		CalculateFlooding(barIndex);

		CalculatePriceExcursions(barIndex);
	}

    public override void OnRender(IDrawingContext drawingContext)
	{
		RenderPriceExcursions(drawingContext);
	}
}
