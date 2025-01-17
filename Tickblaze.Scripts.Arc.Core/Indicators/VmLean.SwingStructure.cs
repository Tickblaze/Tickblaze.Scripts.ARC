using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[AllowNull]
	private Swings _swings;

	[AllowNull]
	private AverageTrueRange _swingDeviationAtr;

	private ISeries<double> SwingDeviationAtr => _swingDeviationAtr.Result;

	[Parameter("Enable Swing Calculations", GroupName = "Swing Structure Parameters", Description = "Whether swings are enabled")]
	public bool IsSwingEnabled { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 200)]
	[Parameter("Swing Strength", GroupName = "Swing Structure Parameters", Description = "Bar lookback to calculate swing high or low")]
	public int SwingStrength { get; set; } = 3;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Deviation Multiplier", GroupName = "Swing Structure Parameters", Description = "ATR multipler of the minimum deviation required for a trend change")]
	public double SwingDeviationAtrMultiplier { get; set; }

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Double Top/Bottom ATR Multiplier", GroupName = "Swing Structure Parameters", Description = "ATR multiplier of the maximum deviation ignored for a double tops or bottoms recognition")]
	public double SwingDtbAtrMultiplier { get; set; }

	[Parameter("Show Swing Dots", GroupName = "Swing Structure Visuals", Description = "Whether swing dots are shown")]
	public bool ShowSwingDots { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 25)]
	[Parameter("Swing Dot Size", GroupName = "Swing Structure Visuals", Description = "Size of the swing dots")]
	public int SwingDotSize { get; set; } = 7;

	[Parameter("Show Swing Labels", GroupName = "Swing Structure Visuals", Description = "Whether swing labels are shown")]
	public bool ShowSwingLabels { get; set; }

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Label Font", GroupName = "Swing Structure Visuals", Description = "Font of the swing labels")]
	public Font SwingLabelFont { get; set; } = new("Arial", 10);

	[Parameter("Up-trend Swing Label Color", GroupName = "Swing Structure Visuals", Description = "Color of the up-trend swing label")]
	public Color SwingUpLabelColor { get; set; } = Color.Black;

	[Parameter("Down-trend Swing Label Color", GroupName = "Swing Structure Visuals", Description = "Color of the down-trend swing label")]
	public Color SwingDownLabelColor { get; set; } = Color.Black;

	[Parameter("Double Top/Bottom Swing Label Color", GroupName = "Swing Structure Visuals", Description = "Color of the double top/bottom swing label")]
	public Color SwingDtbLabelColor { get; set; } = Color.Black;

	[Parameter("Show Swing Lines", GroupName = "Swing Structure Visuals", Description = "Whether swing lines are shown")]
	public bool ShowSwingLines { get; set; }

	[Parameter("Up-trend Swing Line Color", GroupName = "Swing Structure Visuals", Description = "Color of the up-trend swing line")]
	public Color SwingUpLineColor { get; set; } = Color.Black;

	[Parameter("Down-trend Swing Line Color", GroupName = "Swing Structure Visuals", Description = "Color of the down-trend swing line")]
	public Color SwingDownLineColor { get; set; } = Color.Black;

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Line Thickness", GroupName = "Swing Structure Visuals", Description = "Thickness of the swing lines")]
	public int SwingLineThickness { get; set; } = 2;

	[Parameter("Swing Line Style", GroupName = "Swing Structure Visuals", Description = "Line style of the swing lines")]
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
				nameof(SwingDtbLabelColor),
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
			];

			parameters.RemoveRange(propertyNames);
		}
	}

	public Swings InitializeSwings()
	{
		_swingDeviationAtr = new AverageTrueRange
		{
			Bars = Bars,
			Period = 256,
			SmoothingType = MovingAverageType.Simple,
		};

        _swings = new Swings
        {
			Bars = Bars,
			RenderTarget = this,
			DotSize = SwingDotSize,
			ShowDots = ShowSwingDots,
			LabelFont = SwingLabelFont,
			LineStyle = SwingLineStyle,
			ShowLines = ShowSwingLines,
			ShowLabels = ShowSwingLabels,
			SwingStrength = SwingStrength,
			IsDtbLabelColorEnabled = true,
			UpLineColor = SwingUpLineColor,
			IsSwingEnabled = IsSwingEnabled,
			UpLabelColor = SwingUpLabelColor,
			DownLineColor = SwingDownLineColor,
			LineThickness = SwingLineThickness,
			DtbLabelColor = SwingDtbLabelColor,
			DownLabelColor = SwingDownLabelColor,
			CalculationMode = SwingCalculationMode.CurrentBar,
			SwingDtbDeviation = SwingDeviationAtr.Map(atr => SwingDtbAtrMultiplier * atr),
			SwingDeviation = SwingDeviationAtr.Map(atr => SwingDeviationAtrMultiplier * atr),
		};
		
		_swings.Reinitialize();

		return _swings;
	}

	public void CalculateSwings(int barIndex)
	{
		_swings.Calculate();
	}

	public void RenderSwings(IDrawingContext drawingContext)
	{
		//_swings.OnRender(drawingContext);
	}
}