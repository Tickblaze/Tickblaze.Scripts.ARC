﻿using Tickblaze.Scripts.Api.Models;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.BarTypes;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: RenkoBXT part.
// Todo: price markers support.
// Todo: parameter descriptions.
// Todo: fix realtime check.
public partial class BarCloseMarker : Indicator
{
	public BarCloseMarker()
	{
		IsOverlay = true;
		
		ShortName = "BCM";
		
		Name = "Bar Close Marker";
	}

	private double _lastOpen;
	private double _lastClose;
	private double _potentialLow;
	private double _potentialHigh;

	private RenkoBxt? _renkoBxt;

	private Bar? _lastBar;

	private Color _markerLowSolidColor;
	private Color _markerHighSolidColor;

	[Parameter("Marker Type")]
	public MarkerType MarkerTypeValue { get; set; } = MarkerType.ExtendedLines;

	[Parameter("Display Shadow", GroupName = "Visuals")]
	public bool ShowShadows { get; set; } = true;
	
	[Parameter("Text Font", GroupName = "Visuals")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[NumericRange(MinValue = 1)]
	[Parameter("Marker Width", GroupName = "Visuals")]
	public int MarkerThickness { get; set; } = 2;

	[Parameter("Bar Close High Color", GroupName = "Visuals")]
	public Color MarkerHighColor { get; set; } = DrawingColor.Lime.With(0.5f);

	[Parameter("Current Price Color", GroupName = "Visuals")]
	public Color CurrentPriceColor { get; set; } = Color.Black.With(opacity: 0.5f);

	[Parameter("Bar Close Low Color", GroupName = "Visuals")]
	public Color MarkerLowColor { get; set; } = Color.Red.With(opacity: 0.5f);

	[Parameter("Top Shadow Color", GroupName = "Visuals")]
	public Color TopShadowColor { get; set; } = DrawingColor.Lime.With(0.1f);

	[Parameter("Bottom Shadow Color", GroupName = "Visuals")]
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
		_lastBar = default;
		_lastOpen = default;
		_lastClose = default;

		_renkoBxt = Bars.BarType as RenkoBxt;

		_markerLowSolidColor = MarkerLowColor.With(opacity: 1.0f);
		_markerHighSolidColor = MarkerHighColor.With(opacity: 1.0f);
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

		_lastBar = Bars[barIndex]!;
		_lastOpen = _lastBar.Open;
		_lastClose = _lastBar.Close;

		var tickSize = Bars.Symbol.TickSize;
		var barSizeInPoints = _renkoBxt.BarSize * tickSize;
		var reversalSizeInPoints = _renkoBxt.ReversalSize * tickSize;
		
		var isUpTrend = Bars.Close[barIndex - 1] > Bars.Open[barIndex - 1];

		if (isUpTrend)
		{
			_potentialHigh = Bars.Open[barIndex] + barSizeInPoints;

			_potentialLow = Bars.High[barIndex] - reversalSizeInPoints;
		}
		else
		{
			_potentialHigh = Bars.Open[barIndex] - barSizeInPoints;

			_potentialLow = Bars.Low[barIndex] + reversalSizeInPoints;
		}
    }

    private void CalculateRangePotentialHighLow(int barIndex)
	{
		var bar = Bars[barIndex]!;
		var barTypeSettings = Bars.Period;
		var tickSize = Bars.Symbol.TickSize;
		
		var rangeDelta = barTypeSettings.Size * tickSize;
		var currentDelta = Math.Abs(bar.Close - bar.Open);

		if (currentDelta.ApproxGreaterThanOrEquals(rangeDelta))
		{
			return;
		}

		_lastBar = bar;
		_lastOpen = _lastBar.Open;
		_lastClose = _lastBar.Close;

		_potentialLow = Math.Round(_lastOpen - rangeDelta, Symbol.Decimals);
		_potentialHigh = Math.Round(_lastOpen + rangeDelta, Symbol.Decimals);
	}

	public override void OnRender(IDrawingContext context)
    {
		if (_lastBar is null)
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
			context.DrawHorizontalRay(barStartX, potentialLowY, HorizontalDirection.Right, MarkerLowColor, MarkerThickness);
			context.DrawHorizontalRay(barStartX, lastCloseY, HorizontalDirection.Right, CurrentPriceColor, MarkerThickness);
			context.DrawHorizontalRay(barStartX, potentialHighY, HorizontalDirection.Right, MarkerHighColor, MarkerThickness);

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
