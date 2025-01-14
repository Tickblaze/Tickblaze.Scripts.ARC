using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

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

	[Parameter("Menu Header", Description = "Menu settings header")]
	public string MenuHeader { get; set; } = "VM Lean";

	[Plot("Zero Line")]
	public PlotLevel ZeroLine { get; set; } = new(0.0, Color.Black, LineStyle.Solid, 2);

	public override object? CreateChartToolbarMenuItem()
	{
		var uri = new Uri("/Tickblaze.Scripts.Arc.Core;component/Indicators/VmLean.Menu.xaml", UriKind.Relative);

		var menuObject = Application.LoadComponent(uri);

		if (menuObject is Menu menu)
		{
			menu.DataContext = _menuViewModel;
		}

		return menuObject;
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
