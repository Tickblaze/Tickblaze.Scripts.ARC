using Tickblaze.Scripts.Arc.Domain;

namespace Tickblaze.Scripts.Arc;

// Todo: quick access settings.
public partial class SwingStructure : Indicator
{
	public SwingStructure()
	{
		IsOverlay = true;
		ShortName = "TBC SS";
		Name = "TB Core Swing Structure";
	}

	private SwingContainer _swingContainer = default!;

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

	[Parameter("Menu Header", Description = "Quick access menu header")]
	public string MenuHeader { get; set; } = "TBC Swing";

	protected override Parameters GetParameters(Parameters parameters)
    {
		if (!ShowSwingLines)
		{
			string[] swingLinePropertyNames =
			[
				nameof(LabelFont),
				nameof(UpLineColor),
				nameof(DownLineColor),
				nameof(LineThickness),
				nameof(ShowSwingLabels),
			];

			Array.ForEach(swingLinePropertyNames, propertyName => parameters.Remove(propertyName));
		}

		if (!ShowSwingLabels)
		{
			parameters.Remove(nameof(LabelFont));
		}

		return parameters;
    }

	protected override void Initialize()
    {
		_swingContainer?.Clear();

		_swingContainer = new SwingContainer
		{
			BarSeries = Bars,
			SwingStrength = SwingStrength,
			CalculationMode = CalculationMode,
		};
    }

    protected override void Calculate(int index)
    {
		_swingContainer.CalculateSwings(index);
    }

    public override void OnRender(IDrawingContext context)
    {
		if (!ShowSwingLines)
		{
			return;
		}

		var visibleRectangle = this.GetVisibleRectangle();
		var visibleSwings = _swingContainer.GetVisibleComponents(visibleRectangle);

		foreach (var visibleSwing in visibleSwings)
		{
			var endPoint = this.ToApiPoint(visibleSwing.EndPoint);
			var startPoint = this.ToApiPoint(visibleSwing.StartPoint);
			var color = visibleSwing.Trend.Map(UpLineColor, DownLineColor);

			context.DrawLine(startPoint, endPoint, color, LineThickness);

			if (ShowSwingLabels)
			{
				var label = visibleSwing.Label.ShortName;
				var labelSize = context.MeasureText(label, LabelFont);
				var labelVerticalOffset = visibleSwing.Trend.Map(-TextVerticalOffset - labelSize.Height, TextVerticalOffset);
				var labelHorizontalOffset = -labelSize.Width / 2.0;

				endPoint.Y += labelVerticalOffset;
				endPoint.X += labelHorizontalOffset;

				context.DrawText(endPoint, label, color, LabelFont);
			}
		}
    }
}
