﻿using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: quick access settings.
public partial class SwingStructure : Indicator
{
	public SwingStructure()
	{
		_menuViewModel = new(this);

		IsOverlay = true;
		ShortName = "SS";
		Name = "Swing Structure";
	}

	[AllowNull]
	private Swings _swings;

	private readonly MenuViewModel _menuViewModel;

	[Parameter("Calculation Mode", Description = "Whether to calculate the structure by current or closed bar highs and lows")]
	public SwingCalculationMode CalculationMode { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 200)]
	[Parameter("Swing strength", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 1;

	[Parameter("Show Lines", Description = "Show structure trend lines")]
	public bool ShowSwingLines { get; set; } = true;

	[Parameter("Up-trend line color", Description = "Line color for up-trending structure")]
	public Color UpLineColor { get; set; } = DrawingColor.DarkGreen;

	[Parameter("Down-trend line color", Description = "Line color for down-trending structure")]
	public Color DownLineColor { get; set; } = Color.Red;

	[NumericRange(MinValue = 1, MaxValue = 10)]
	[Parameter("Line Thickness", Description = "Thickness of structure lines")]
	public int LineThickness { get; set; } = 2;

	[Parameter("Show Labels", Description = "Whether structure labels such as 'HH' and 'LH' need to be shown")]
	public bool ShowSwingLabels { get; set; }

	[Parameter("Label Font", Description = "Font for structure labels")]
	public Font LabelFont { get; set; } = new("Arial", 12);

	[Parameter("Settings Header", Description = "Quick access settings header")]
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
		var uri = new Uri("/Tickblaze.Scripts.Arc.Core;component/Indicators/SwingStructure.Menu.xaml", UriKind.Relative);

		var menuObject = Application.LoadComponent(uri);

		if (menuObject is Menu menu)
		{
			menu.DataContext = _menuViewModel;
		}

		return menuObject;
	}

    protected override void Initialize()
    {
		_menuViewModel.Initialize();

		_swings = new Swings
		{
			ShowDots = false,
			RenderTarget = this,
			IsSwingEnabled = true,
			LabelFont = LabelFont,
			UpLineColor = UpLineColor,
			ShowLines = ShowSwingLines,
			LineStyle = LineStyle.Solid,
			ShowLabels = ShowSwingLabels,
			SwingStrength = SwingStrength,
			DownLineColor = DownLineColor,
			LineThickness = LineThickness,
			CalculationMode = CalculationMode,
		};
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
