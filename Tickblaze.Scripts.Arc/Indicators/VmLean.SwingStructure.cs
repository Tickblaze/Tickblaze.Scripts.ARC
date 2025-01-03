using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	private SwingContainer _swingContainer = default!;
	private AverageTrueRange _swingDeviationAtr = new();

	private ISeries<double> SwingDeviationAtr => _swingDeviationAtr.Result;

	[Parameter("Enable Swing Calculations", GroupName = "Swing Structure Parameters", Description = "Whether swings are enabled")]
	public bool IsSwingEnabled { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Strength", GroupName = "Swing Structure Parameters", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 3;

	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("Swing Deviation Multiplier", GroupName = "Swing Structure Parameters", Description = "Multiplier used to calculate minimum deviation as an ATR multiple")]
	public double SwingDeviationAtrMultiplier { get; set; }

	[NumericRange(MaxValue = double.MaxValue)]
	[Parameter("Swing Sensitivity Double Tops/Bottoms", GroupName = "Swing Structure Parameters", Description = "Fraction of ATR ignored when detecting double tops or bottoms")]
	public double DoubleTopBottomAtrMultiplier { get; set; }

	[Parameter("Show Swing Dots", GroupName = "Swing Structure Parameters", Description = "Whether swing dots are shown")]
	public bool ShowSwingDots { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 25)]
	[Parameter("Swing Dot Size", GroupName = "Swing Structure Parameters", Description = "Size of dots representing swing highs and lows")]
	public int SwingDotSize { get; set; } = 7;

	[Parameter("Show Swing Labels", GroupName = "Swing Structure Parameters", Description = "Whether swing labels are shown")]
	public bool ShowSwingLabels { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Label Font", GroupName = "Swing Structure Parameters", Description = "Label font for swing highs and lows")]
	public Font SwingLabelFont { get; set; } = new("Arial", 10);

	[Parameter("Rising Swing Label Color", GroupName = "Swing Structure Parameters")]
	public Color SwingUpLabelColor { get; set; } = Color.Black;

	[Parameter("Falling Swing Label Color", GroupName = "Swing Structure Parameters")]
	public Color SwingDownLabelColor { get; set; } = Color.Black;

	[Parameter("Show Swing Lines", GroupName = "Swing Structure Parameters", Description = "Whether swing lines are shown")]
	public bool ShowSwingLines { get; set; }

	[Parameter("Rising Swing Line Color", GroupName = "Swing Structure Parameters")]
	public Color SwingUpLineColor { get; set; } = Color.Black;

	[Parameter("Falling Swing Line Color", GroupName = "Swing Structure Parameters")]
	public Color SwingDownLineColor { get; set; } = Color.Black;

	[Parameter("Double Top/Bottom Swing Line Color", GroupName = "Swing Structure Parameters")]
	public Color SwingDoubleTopBottomLineColor { get; set; } = Color.Black;

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Line Thickness", GroupName = "Swing Structure Parameters", Description = "Thickness of lines connecting swing highs lows")]
	public int SwingLineThickness { get; set; } = 2;

	[Parameter("Swing Line Style", GroupName = "Swing Structure Parameters", Description = "Style of lines connecting swing highs lows")]
	public LineStyle SwingLineStyle { get; set; } = LineStyle.Solid;

	public void HideSwingParameters(Parameters parameters)
	{
		if (!ShowSwingDots)
		{
			parameters.Remove(nameof(SwingDotSize));
		}

		if (!ShowSwingLabels)
		{
			ReadOnlySpan<string> propertyNames =
			[
				nameof(SwingLabelFont),
				nameof(SwingUpLabelColor),
				nameof(SwingDownLabelColor),
			];

			parameters.RemoveRange(propertyNames);
		}

		if (!ShowSwingLines)
		{
			ReadOnlySpan<string> propertyNames =
			[
				nameof(SwingLineStyle),
				nameof(SwingUpLineColor),
				nameof(SwingDownLineColor),
				nameof(SwingLineThickness),
				nameof(SwingDoubleTopBottomLineColor),
			];

			parameters.RemoveRange(propertyNames);
		}
	}

	public void InitializeSwings()
	{
		_swingDeviationAtr = new(256, MovingAverageType.Simple);

		_swingContainer = new SwingContainer
		{
			BarSeries = Bars,
			SwingStrength = SwingStrength,
			CalculationMode = SwingCalculationMode.CurrentBar,
			SwingDeviation = SwingDeviationAtr.Map(atr => SwingDeviationAtrMultiplier * atr),
			DoubleTopBottomDeviation = SwingDeviationAtr.Map(atr => DoubleTopBottomAtrMultiplier * atr),
		};
	}

	public void CalculateSwings(int barIndex)
	{
		_swingContainer.CalculateSwings(barIndex);
	}
}
