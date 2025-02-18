using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace Tickblaze.Community;

public partial class SwingStructure : Indicator
{
	public SwingStructure()
	{
		_menuViewModel = new(this);

		IsOverlay = true;
		
		ShortName = "SS";
		
		Name = "Swing Structure";
	}

	private readonly Lock _lock = new();

	[AllowNull]
	private Swings _swings;

	private readonly MenuViewModel _menuViewModel;

	private const string _menuResourceName = "Tickblaze.Community.SwingStructure.SwingStructure.Menu.xaml";

	[Parameter("Calculation Mode", Description = "Whether to calculate the swing by current or closed bar highs and lows")]
	public SwingCalculationMode CalculationMode { get; set; }

	[NumericRange(MinValue = Swings.SwingStrengthMin, MaxValue = Swings.SwingStrengthMax)]
	[Parameter("Swing Strength", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 1;

	[NumericRange(MaxValue = double.MaxValue, Step = DoubleStep)]
	[Parameter("Swing Deviation Multiplier", Description = "ATR multipler of the minimum deviation required for a trend change")]
	public double SwingDeviationAtrMultiplier { get; set; }

	[NumericRange(MaxValue = double.MaxValue, Step = DoubleStep)]
	[Parameter("Swing Double Top/Bottom ATR Multiplier", Description = "ATR multiplier of the maximum deviation ignored for a double tops or bottoms recognition")]
	public double SwingDtbAtrMultiplier { get; set; }

	[Parameter("Show Lines", Description = "Whether swing lines are shown")]
	public bool ShowSwingLines { get; set; } = true;

	[Parameter("Up-trend line color", Description = "Color of the up-trending swing line")]
	public Color UpLineColor { get; set; } = DrawingColor.DarkGreen;

	[Parameter("Down-trend line color", Description = "Color of the down-trending swing line")]
	public Color DownLineColor { get; set; } = Color.Red;

	[NumericRange(MinValue = ThicknessMin, MaxValue = ThicknessMax)]
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

        _menuViewModel.Initialize();
	}

	private void InitializeSwings(bool forceReinitialization)
	{
		using var lockScope = _lock.EnterScope();

		_swings = new Swings
		{
			Bars = Bars,
			ShowDots = false,
			RenderTarget = this,
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
			SwingDtbAtrMultiplier = SwingDtbAtrMultiplier,
			SwingDeviationAtrMultiplier = SwingDeviationAtrMultiplier,
		};

		if (forceReinitialization)
		{
			_swings.Reinitialize(this);
		}
	}

	protected override void Calculate(int index)
	{
		using var lockScope = _lock.EnterScope();

		_swings.Calculate();
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		_swings.OnRender(drawingContext);
	}
}
