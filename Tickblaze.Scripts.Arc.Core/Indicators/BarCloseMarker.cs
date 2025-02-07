using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.BarTypes;

namespace Tickblaze.Scripts.Arc.Core;

public partial class BarCloseMarker : Indicator
{
	public BarCloseMarker()
	{
		IsOverlay = true;
		
		ShortName = "BCM";
		
		Name = "Bar Close Marker";
	}

	private RenkoBxt? _renkoBxt;
	
	private int _currentBarIndex;

	private Color _markerLowSolidColor;
	private Color _markerHighSolidColor;

	[Parameter("Marker Type", Description = "Type of the marker")]
	public MarkerType MarkerTypeValue { get; set; } = MarkerType.ExtendedLines;

	[NumericRange(MinValue = ThicknessMin, MaxValue = ThicknessMax)]
	[Parameter("Marker Width", GroupName = "Visuals", Description = "Width of the marker")]
	public int MarkerThickness { get; set; } = 2;

	[Parameter("Display Shadow", GroupName = "Visuals", Description = "Whether shadows are shown")]
	public bool ShowShadows { get; set; } = true;
	
	[Parameter("Text Font", GroupName = "Visuals", Description = "Font of the text")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Parameter("Bar Close High Color", GroupName = "Visuals", Description = "Color of the bar close high")]
	public Color MarkerHighColor { get; set; } = DrawingColor.Lime.With(0.5f);

	[Parameter("Current Price Color", GroupName = "Visuals", Description = "Color of the current price")]
	public Color CurrentPriceColor { get; set; } = Color.Black.With(opacity: 0.5f);

	[Parameter("Bar Close Low Color", GroupName = "Visuals", Description = "Color of the bar close low")]
	public Color MarkerLowColor { get; set; } = Color.Red.With(opacity: 0.5f);

	[Parameter("Top Shadow Color", GroupName = "Visuals", Description = "Color of the top shadow")]
	public Color TopShadowColor { get; set; } = DrawingColor.Lime.With(0.1f);

	[Parameter("Bottom Shadow Color", GroupName = "Visuals", Description = "Color of the bottom shadow")]
	public Color BottomShadowColor { get; set; } = Color.Red.With(opacity: 0.1f);

	public PlotSeries PotentialHigh { get; init; } = new("Potential High", Color.Transparent, isVisible: false);

	public PlotSeries PotentialLow { get; init; } = new("Potential High", Color.Transparent, isVisible: false);

	protected override Parameters GetParameters(Parameters parameters)
    {
		if (MarkerTypeValue is MarkerType.None)
		{
			ReadOnlySpan<string> propertyNames =
			[
				nameof(TextFont),
				nameof(MarkerHighColor),
				nameof(CurrentPriceColor),
				nameof(MarkerLowColor),
				nameof(MarkerThickness),
			];

			parameters.RemoveRange(propertyNames);
		}

		if (MarkerTypeValue is MarkerType.Price)
		{
			parameters.Remove(nameof(CurrentPriceColor));
		}

		if (MarkerTypeValue is MarkerType.ExtendedLines)
		{
			parameters.Remove(nameof(TextFont));
		}

		if (!ShowShadows)
		{
			parameters.RemoveRange([nameof(TopShadowColor), nameof(BottomShadowColor)]);
		}

        return base.GetParameters(parameters);
    }

    protected override void Initialize()
    {
		_currentBarIndex = -1;

		_renkoBxt = Bars.BarType as RenkoBxt;

		PotentialLow.Color = _markerLowSolidColor = MarkerLowColor.With(opacity: 1.0f);
		PotentialHigh.Color = _markerHighSolidColor = MarkerHighColor.With(opacity: 1.0f);
	}

    protected override void Calculate(int barIndex)
    {
		var barTypeSettings = Bars.Period;

		if (_renkoBxt is not null)
		{
			CalculateRenkoBxtPotentialHighLow(barIndex);
		}

		if (barTypeSettings.Type is BarType.Range)
		{
			CalculateRangePotentialHighLow(barIndex);
		}
	}

    private void CalculateRenkoBxtPotentialHighLow(int barIndex)
    {
		if (_renkoBxt is null || barIndex is 0)
        {
			return;
		}

		_currentBarIndex = barIndex;

		var tickSize = Bars.Symbol.TickSize;
		var barSizeInPoints = _renkoBxt.BarSize * tickSize;
		var reversalSizeInPoints = _renkoBxt.ReversalSize * tickSize;
		
		var isUpTrend = Bars.Close[barIndex - 1] > Bars.Open[barIndex - 1];

		if (isUpTrend)
		{
			PotentialLow[barIndex] = Symbol.RoundToTick(Bars.High[barIndex] - reversalSizeInPoints);

			PotentialHigh[barIndex] = Symbol.RoundToTick(Bars.Open[barIndex] + barSizeInPoints);
		}
		else
		{
			PotentialLow[barIndex] = Symbol.RoundToTick(Bars.Open[barIndex] - barSizeInPoints);
			
			PotentialHigh[barIndex] = Symbol.RoundToTick(Bars.Low[barIndex] + reversalSizeInPoints);
		}
	}

    private void CalculateRangePotentialHighLow(int barIndex)
	{
		var bar = Bars[barIndex];
		var barTypeSettings = Bars.Period;
		var tickSize = Bars.Symbol.TickSize;
		
		var currentDelta = bar.High - bar.Low;

		var rangeDelta = barTypeSettings.Size * tickSize;

		var remainingDelta = rangeDelta - currentDelta;

		if (remainingDelta.ApproxLessThanOrEquals(0))
		{
			return;
		}

		_currentBarIndex = barIndex;
		
		PotentialLow[barIndex] = Symbol.RoundToTick(bar.Low - remainingDelta);
		
		PotentialHigh[barIndex] = Symbol.RoundToTick(bar.High + remainingDelta);
	}

	public override void OnRender(IDrawingContext context)
    {
		if (_currentBarIndex is -1)
		{
			return;
		}

		var currentOpen = Bars.Open[_currentBarIndex];
		var currentClose = Bars.Close[_currentBarIndex];
		var currentCloseY = ChartScale.GetYCoordinateByValue(currentClose);
		
		var candleLow = Math.Min(currentOpen, currentClose);
		var candleLowY = ChartScale.GetYCoordinateByValue(candleLow);
		
		var potentialLow = PotentialLow.GetLastOrDefault();
		var potentialLowY = ChartScale.GetYCoordinateByValue(potentialLow);

		var candleHigh = Math.Max(currentOpen, currentClose);
		var candleHighY = ChartScale.GetYCoordinateByValue(candleHigh);

		var potentialHigh = PotentialHigh.GetLastOrDefault();
		var potentialHighY = ChartScale.GetYCoordinateByValue(potentialHigh);
		
		var absoluteBarWidth = Chart.GetAbsoluteBarWidth();
		var barCenterX = Chart.GetXCoordinateByBarIndex(Bars.Count - 1);
		var barStartX = barCenterX - absoluteBarWidth / 2.0;
		var barEndX = barCenterX + absoluteBarWidth / 2.0;

		if (ShowShadows)
		{
			context.DrawRectangle(barStartX, potentialHighY, barEndX, candleHighY, TopShadowColor);
			context.DrawRectangle(barStartX, candleLowY, barEndX, potentialLowY, BottomShadowColor);
		}

		if (MarkerTypeValue is MarkerType.ExtendedLines)
		{
			context.DrawHorizontalRay(barStartX, potentialLowY, HorizontalDirection.Right, MarkerLowColor, MarkerThickness);
			context.DrawHorizontalRay(barStartX, currentCloseY, HorizontalDirection.Right, CurrentPriceColor, MarkerThickness);
			context.DrawHorizontalRay(barStartX, potentialHighY, HorizontalDirection.Right, MarkerHighColor, MarkerThickness);

			context.DrawHorizontalLine(barStartX, potentialLowY, barEndX, _markerLowSolidColor, MarkerThickness);
			context.DrawHorizontalLine(barStartX, potentialHighY, barEndX, _markerHighSolidColor, MarkerThickness);
		}

		if (MarkerTypeValue is MarkerType.Price)
		{
			var potentialLowText = potentialLow.ToString();
			var potentialHighText = potentialHigh.ToString();
			var potentialLowTextSize = context.MeasureText(potentialLowText, TextFont);
			var potentialHighTextSize = context.MeasureText(potentialHighText, TextFont);

			var maxTextWidth = Math.Max(potentialLowTextSize.Width, potentialHighTextSize.Width);

			if (maxTextWidth <= absoluteBarWidth)
			{
				var potentialLowTextX = barCenterX - potentialLowTextSize.Width / 2.0;
				var potentialLowTextY = potentialLowY + VerticalMargin;

				var potentialHighTextX = barCenterX - potentialHighTextSize.Width / 2.0;
				var potentialHighTextY = potentialHighY - VerticalMargin - potentialHighTextSize.Height;

				context.DrawText(potentialLowTextX, potentialLowTextY, potentialLowText, _markerLowSolidColor, TextFont);
				context.DrawText(potentialHighTextX, potentialHighTextY, potentialHighText, _markerHighSolidColor, TextFont);
			}

			context.DrawHorizontalLine(barStartX, potentialLowY, barEndX, MarkerLowColor, MarkerThickness);
			context.DrawHorizontalLine(barStartX, potentialHighY, barEndX, MarkerHighColor, MarkerThickness);
		}
	}

	public enum MarkerType
	{
		[DisplayName("None")]
		None,

		[DisplayName("Price")]
		Price,

		[DisplayName("Extended Lines")]
		ExtendedLines,
	}
}
