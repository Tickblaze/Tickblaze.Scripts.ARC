using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: separate paremeters and their visuals.
public partial class VmLean : Indicator
{
	public VmLean()
	{
		Name = "VM Lean";
		ShortName = "VML";
	}

	[AllowNull]
	private MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Tickblaze.Scripts.Arc.Core.Indicators.VmLean.Menu.xaml";

	[Parameter("Menu Header", Description = "Menu settings header")]
	public string MenuHeader { get; set; } = "VM Lean";

	[Plot("Zero Line")]
	public PlotLevel ZeroLine { get; set; } = new(0.0, Color.Black, LineStyle.Solid, 2);

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
		InitializeSwings(false);

		InitializeMacdBb();

		InitializeHistogram();

		InitializeFlooding();

		_menuViewModel = new(this);
	}

	protected override void Calculate(int barIndex)
	{
		CalculateHistogram(barIndex);

		CalculateMacdBb(barIndex);

		CalculateSwings(barIndex);

		CalculateFlooding(barIndex);
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		RenderFlooding(drawingContext);

		RenderMacdBb(drawingContext);
		
		RenderPriceExcursions(drawingContext);

		RenderSentimentBox(drawingContext);
	}
}
