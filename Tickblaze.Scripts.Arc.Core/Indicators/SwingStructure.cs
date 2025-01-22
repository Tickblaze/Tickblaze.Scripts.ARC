using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class SwingStructure : Indicator
{
	public SwingStructure()
	{
		IsOverlay = true;
		
		ShortName = "SS";
		
		Name = "Swing Structure";
	}

	[AllowNull]
	private Swings _swings;

	[AllowNull]
	private MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Tickblaze.Scripts.Arc.Core.Indicators.SwingStructure.Menu.xaml";

	[Parameter("Calculation Mode", Description = "Whether to calculate the swing by current or closed bar highs and lows")]
	public SwingCalculationMode CalculationMode { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 256)]
	[Parameter("Swing Strength", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 1;

	[Parameter("Show Lines", Description = "Whether swing lines are shown")]
	public bool ShowSwingLines { get; set; } = true;

	[Parameter("Up-trend line color", Description = "Color of the up-trending swing line")]
	public Color UpLineColor { get; set; } = DrawingColor.DarkGreen;

	[Parameter("Down-trend line color", Description = "Color of the down-trending swing line")]
	public Color DownLineColor { get; set; } = Color.Red;

	[NumericRange(MinValue = 1, MaxValue = 10)]
	[Parameter("Line Thickness", Description = "Thickness of the swing lines")]
	public int LineThickness { get; set; } = 3;

	[Parameter("Show Labels", Description = "Whether swing labels such as 'HH' and 'LH' are shown")]
	public bool ShowSwingLabels { get; set; }

	[Parameter("Label Font", Description = "Font of the swing labels")]
	public Font LabelFont { get; set; } = new("Arial", 12);

	[Parameter("Menu Header", Description = "Header of the menu")]
	public string MenuHeader { get; set; } = "Swing";

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (!ShowSwingLines)
		{
			ReadOnlySpan<string> propertyNames =
			[
				nameof(LabelFont),
				nameof(UpLineColor),
				nameof(DownLineColor),
				nameof(LineThickness),
				nameof(ShowSwingLabels),
			];

			parameters.RemoveRange(propertyNames);
		}

		if (!ShowSwingLabels)
		{
			parameters.Remove(nameof(LabelFont));
		}

		return parameters;
	}

	public override object? CreateChartToolbarMenuItem()
	{
		var menu = ResourceDictionaries.LoadResource<Menu>(_menuResourceName);

		menu.DataContext = _menuViewModel;

		return menu;
	}

	protected override void Initialize()
	{
		InitializeSwings(false);

		_menuViewModel = new(this);
	}

	private Swings InitializeSwings(bool forceReinitialization)
	{
		_swings = new Swings
		{
			Bars = Bars,
			ShowDots = false,
			RenderTarget = this,
			IsSwingEnabled = true,
			LabelFont = LabelFont,
			UpLineColor = UpLineColor,
			UpLabelColor = UpLineColor,
			ShowLines = ShowSwingLines,
			LineStyle = LineStyle.Solid,
			ShowLabels = ShowSwingLabels,
			SwingStrength = SwingStrength,
			DownLineColor = DownLineColor,
			LineThickness = LineThickness,
			DownLabelColor = DownLineColor,
			IsDtbLabelColorEnabled = false,
			CalculationMode = CalculationMode,
		};

		if (forceReinitialization)
		{
			_swings.Reinitialize();
		}

		return _swings;
	}

	protected override void Calculate(int index)
	{
		_swings.Calculate();
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		_swings.OnRender(drawingContext);
	}
}
