using Tickblaze.Scripts.Arc.Domain;

namespace Tickblaze.Scripts.Arc;

// Todo: RenkoBXT part.
// Todo: parameter descriptions.
public partial class BarCloseMarker : Indicator
{
	public BarCloseMarker()
	{
		IsOverlay = true;
		ShortName = "TBC BCM";
		Name = "TB Core Bar Close Marker";
	}

	private double _lastOpen;
	private double _lastClose;
	private double _potentialLow;
	private double _potentialHigh;

	private Bar? _lastRealtimeBar;

	private Color _markerLowSolidColor;
	private Color _markerHighSolidColor;

	private DateTime _realtimeThresholdUtc;

	[Parameter("MarkerType")]
	public MarkerType MarkerTypeValue { get; set; } = MarkerType.ExtendedLines;

	[Parameter("Text Font")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Parameter("Bar Close High Color")]
	public Color MarkerHighColor { get; set; } = DrawingColor.Lime.ToApiColor(0.5f);

	[Parameter("Current Price Color")]
	public Color CurrentPriceColor { get; set; } = Color.Black.With(opacity: 0.5f);

	[Parameter("Bar Close Low Color")]
	public Color MarkerLowColor { get; set; } = Color.Red.With(opacity: 0.5f);

	[NumericRange(MinValue = 1)]
	[Parameter("Marker Width")]
	public int MarkerThickness { get; set; } = 2;

	[Parameter("Display Shadow")]
	public bool ShowShadows { get; set; } = true;

	[Parameter("Top Shadow Color")]
	public Color TopShadowColor { get; set; } = DrawingColor.Lime.ToApiColor(0.1f);

	[Parameter("Bottom Shadow Color")]
	public Color BottomShadowColor { get; set; } = Color.Red.With(opacity: 0.1f);

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
		_lastOpen = default;
		_lastClose = default;
		_lastRealtimeBar = default;
		_realtimeThresholdUtc = DateTime.UtcNow;
		_markerLowSolidColor = MarkerLowColor.With(opacity: 1.0f);
		_markerHighSolidColor = MarkerHighColor.With(opacity: 1.0f);
	}

    protected override void Calculate(int index)
    {
        if (Bars is not ([.., var lastCachedBar])
            || lastCachedBar is null
            || lastCachedBar.Time < _realtimeThresholdUtc)
        {
			return;
        }
        
		_lastOpen = lastCachedBar.Open;
        _lastClose = lastCachedBar.Close;
		_lastRealtimeBar = lastCachedBar;

		CalculatePotentialHighLow();
    }

    private void CalculatePotentialHighLow()
    {
		var barTypeSettings = Bars.Period;

		if (barTypeSettings.Type is BarType.Range)
		{
			CalculateRangePotentialHighLow();
		}
	}

	private void CalculateRangePotentialHighLow()
	{
		var barTypeSettings = Bars.Period;
		var tickSize = Bars.Symbol.TickSize;
		var rangeDelta = barTypeSettings.Size * tickSize;
		
		_potentialLow = Math.Max(0.0, _lastOpen - rangeDelta);
		_potentialLow = Math.Round(_potentialLow, Symbol.Decimals);

		_potentialHigh = Math.Round(_lastOpen + rangeDelta, Symbol.Decimals);
	}

	public override void OnRender(IDrawingContext context)
    {
		if (_lastRealtimeBar is null || double.IsInfinity(_potentialHigh))
		{
			return;
		}

		var lastCloseY = ChartScale.GetYCoordinateByValue(_lastClose);
		var potentialLowY = ChartScale.GetYCoordinateByValue(_potentialLow);
		var potentialHighY = ChartScale.GetYCoordinateByValue(_potentialHigh);

		var candleLow = Math.Min(_lastOpen, _lastClose);
		var candleLowY = ChartScale.GetYCoordinateByValue(candleLow);

		var candleHigh = Math.Max(_lastOpen, _lastClose);
		var candleHighY = ChartScale.GetYCoordinateByValue(candleHigh);

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
			context.DrawHorizontalRay(barStartX, potentialLowY, HorizontalDirection.Right, MarkerLowColor);
			context.DrawHorizontalRay(barStartX, lastCloseY, HorizontalDirection.Right, CurrentPriceColor);
			context.DrawHorizontalRay(barStartX, potentialHighY, HorizontalDirection.Right, MarkerHighColor);

			context.DrawHorizontalLine(barStartX, potentialLowY, barEndX, _markerLowSolidColor, MarkerThickness);
			context.DrawHorizontalLine(barStartX, potentialHighY, barEndX, _markerHighSolidColor, MarkerThickness);
		}

		if (MarkerTypeValue is MarkerType.Price)
		{
			var potentialLowText = _potentialLow.ToString();
			var potentialHighText = _potentialHigh.ToString();
			var potentialLowTextSize = context.MeasureText(potentialLowText, TextFont);
			var potentialHighTextSize = context.MeasureText(potentialHighText, TextFont);

			var maxTextWidth = Math.Max(potentialLowTextSize.Width, potentialHighTextSize.Width);

			if (maxTextWidth <= absoluteBarWidth)
			{
				var potentialLowTextX = barCenterX - potentialLowTextSize.Width / 2.0;
				var potentialLowTextY = potentialLowY + TextVerticalOffset;

				var potentialHighTextX = barCenterX - potentialHighTextSize.Width / 2.0;
				var potentialHighTextY = potentialHighY - TextVerticalOffset - potentialHighTextSize.Height;

				context.DrawText(potentialLowTextX, potentialLowTextY, potentialLowText, _markerLowSolidColor, TextFont);
				context.DrawText(potentialHighTextX, potentialHighTextY, potentialHighText, _markerHighSolidColor, TextFont);
			}

			context.DrawHorizontalLine(barStartX, potentialLowY, barEndX, MarkerLowColor, MarkerThickness);
			context.DrawHorizontalLine(barStartX, potentialHighY, barEndX, MarkerHighColor, MarkerThickness);
		}
	}
}
