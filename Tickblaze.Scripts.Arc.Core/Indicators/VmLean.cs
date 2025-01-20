using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean : Indicator
{
	public VmLean()
	{
		Name = "VM Lean";

		ShortName = "VML";
	}

	[AllowNull]
	private VmLeanCore _vmLeanCore;

	[AllowNull]
	private MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Tickblaze.Scripts.Arc.Core.Indicators.VmLean.Menu.xaml";

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
		HideSwingParameters(parameters);

		HidePriceExcursionParameters(parameters);

		return parameters;
	}

	protected override void Initialize()
	{
		_vmLeanCore = new VmLeanCore
		{
			Bars = Bars,
			RenderTarget = this,

			BandPeriod = BandPeriod,
			BandMultiplier = BandMultiplier,
			MacdSlowPeriod = MacdSlowPeriod,
			MacdFastPeriod = MacdFastPeriod,

			SwingStrength = SwingStrength,
			SwingDtbAtrMultiplier = SwingDtbAtrMultiplier,
			SwingDeviationAtrMultiplier = SwingDeviationAtrMultiplier,
		};

		_vmLeanCore.Reinitialize();

		InitializeSwings();

		InitializeMacdBb();

		InitializeFlooding();

		InitializePriceExcursions();

		_menuViewModel = new(this);
	}

	protected override void Calculate(int barIndex)
	{
		// Todo: document this.
		ZeroLine[barIndex] = 0.0;

		_vmLeanCore.Calculate();

		CalculateHistogram(barIndex);

		CalculateMacdBb(barIndex);

		CalculateFlooding(barIndex);

		CalculatePriceExcursions(barIndex);
	}

    public override void OnRender(IDrawingContext drawingContext)
	{
		RenderFlooding(drawingContext);

		RenderMacdBb(drawingContext);
		
		RenderPriceExcursions(drawingContext);

		RenderSentimentBox(drawingContext);

		// RenderSwings(drawingContext);
	}
}
