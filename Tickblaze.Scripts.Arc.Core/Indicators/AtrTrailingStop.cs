using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: parameter descriptions.
// Todo: support for custom input series.
public partial class AtrTrailingStop : Indicator
{
	public AtrTrailingStop()
	{
		IsOverlay = true;
		ShortName = "ATS";
		Name = "ATR Trailing Stop";
	}

	private int _currentIndex;
	
	[AllowNull]
	private Font _markerFont;

	[AllowNull]
	private AverageTrueRange _atr;

	[AllowNull]
	private Series<StrictTrend> _trends;

	[AllowNull]
	private AverageTrueRange _markerDeltaAtr;

	[AllowNull]
	private DrawingPartSet<Point> _markers;
	
	[AllowNull]
	private DrawingPartDictionary<int, TrendInterval> _trendIntervals;

	[Parameter("ATR Period")]
	public int AtrPeriod { get; set; } = 10;

	[Parameter("ATR Multiplier")]
	public double AtrMultiplier { get; set; } = 2.5;

	[Parameter("Enable Tick Rounding")]
	public bool IsTickRoundingEnabled { get; set; }

	[Parameter("Show Stop Line", GroupName = "Visuals")]
	public bool ShowStopLine { get; set; } = true;

	[Parameter("Stop Line Thickness", GroupName = "Visuals")]
	public int StopLineThickness { get; set; } = 1;

	[Parameter("Stop Line Style", GroupName = "Visuals")]
	public LineStyle StopLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show Triangles", GroupName = "Visuals")]
	public bool ShowMarkers { get; set; }

	[Parameter("Triangle Size", GroupName = "Visuals")]
	public int MarkerSize { get; set; } = 10;

	[Parameter("Bullish Color", GroupName = "Visuals")]
	public Color BullishColor { get; set; } = Color.Blue;

	[Parameter("Bearish Color", GroupName = "Visuals")]
	public Color BearishColor { get; set; } = Color.Red;

	[Plot("Trailing Stop Dots")]
	public PlotSeries StopDots { get; set; } = new(Color.Transparent, PlotStyle.Dot, 2);

	[Plot("Trailing Reverse Dots")]
	public PlotSeries ReverseDots { get; set; } = new(Color.Yellow, PlotStyle.Dot, 2)
	{
		IsVisible = false
	};

	private StrictTrend CurrentTrend
	{
		get;
		set
		{
			_trends[_currentIndex] = value;
			
			if (_currentIndex is not 0 && field.EnumEquals(value))
			{
				return;
			}

			field = value;

			AddOrUpdateTrendInterval();

			AddOrUpdateMarker();
		}
	}

    protected override Parameters GetParameters(Parameters parameters)
	{
		HideBandParameters(parameters);

		if (!ShowMarkers)
		{
			parameters.Remove(nameof(MarkerSize));
		}

		if (!ShowStopLine)
		{
			parameters.RemoveRange([nameof(StopLineStyle), nameof(StopLineThickness)]);
		}

		return parameters;
	}

	private void AddOrUpdateTrendInterval()
	{
		var trendInterval = new TrendInterval
		{
			Trend = CurrentTrend,
			EndBarIndex = _currentIndex,
			StartBarIndex = _currentIndex,
		};

		_trendIntervals.AddOrUpdate(trendInterval);
	}

	private void AddOrUpdateMarker()
	{
		if (_currentIndex is 0)
		{
			return;
		}

		var previousIndex = _currentIndex - 1;
		var previousTrend = _trends[previousIndex];

		var markerDeltaAtr = 0.3 * _markerDeltaAtr[previousIndex];
		var markerPrice = previousTrend is StrictTrend.Up
			? Bars.Low[previousIndex] - markerDeltaAtr
			: Bars.High[previousIndex] + markerDeltaAtr;
		var markerPoint = new Point
		{
			Price = markerPrice,
			BarIndex = previousIndex,
		};

		_markers.AddOrUpdate(markerPoint);
	}

	protected override void Initialize()
    {
        InitializeTrailingStops();

        InitializeBands();
    }

    private void InitializeTrailingStops()
    {
        _trends = [];
        _markers = [];
        _trendIntervals = [];

        _currentIndex = default;

        _atr = new(AtrPeriod, MovingAverageType.WellesWilder);
        
        _markerFont = new("Webdings", 3 * MarkerSize);

		_markerDeltaAtr = new(256, MovingAverageType.Simple);
    }

    protected override void Calculate(int barIndex)
    {
        var currentOpen = Bars.Open[barIndex];
        var currentClose = Bars.Close[barIndex];

		if (barIndex is not 0 && _currentIndex.Equals(barIndex))
		{
			return;
		}
		else if (barIndex is 0)
        {
            StopDots[barIndex] = ReverseDots[barIndex] = Bars.Close[barIndex];

            CurrentTrend = currentClose.EpsilonGreaterThanOrEquals(currentOpen) ? StrictTrend.Up : StrictTrend.Down;
        }
		else
		{
			_currentIndex = barIndex;
			
			CalculateTrailingStops(barIndex);
		}

		CalculateBands(barIndex);
    }

    private void CalculateTrailingStops(int barIndex)
    {
        var previousTrend = CurrentTrend;
        var previousBarIndex = barIndex - 1;
        var previousClose = Bars.Close[previousBarIndex];
        var previousTrailingStop = StopDots[previousBarIndex];

        var isTrendBreak = previousTrend is StrictTrend.Down && previousClose > previousTrailingStop
            || previousTrend is StrictTrend.Up && previousClose < previousTrailingStop;

        if (isTrendBreak)
        {
            CurrentTrend = previousTrend.GetOppositeTrend();
        }
        else
        {
            CurrentTrend = previousTrend;

            var lastTrendInterval = _trendIntervals.LastDrawingPart;

            lastTrendInterval.EndBarIndex = _currentIndex;
        }

        var trailingAmountSignum = CurrentTrend.Map(1, -1);
        var trailingAmount = trailingAmountSignum * AtrMultiplier * _atr[previousBarIndex];
        var currentTrailingStop = previousClose - trailingAmount;

        ReverseDots[barIndex] = previousClose + trailingAmount;
        StopDots.Colors[barIndex] = CurrentTrend.Map(BullishColor, BearishColor);
        StopDots[barIndex] = CurrentTrend switch
        {
            _ when isTrendBreak => currentTrailingStop,
            StrictTrend.Up => Math.Max(previousTrailingStop, currentTrailingStop),
            StrictTrend.Down => Math.Min(previousTrailingStop, currentTrailingStop),
            _ => throw new UnreachableException(),
        };

        if (IsTickRoundingEnabled)
        {
            StopDots[barIndex] = Symbol.RoundToTick(StopDots[barIndex]);
            ReverseDots[barIndex] = Symbol.RoundToTick(ReverseDots[barIndex]);
        }
    }

    public override void OnRender(IDrawingContext drawingContext)
    {
		// Todo: test performance with precached canvas points.

		RenderBands(drawingContext);

		RenderTrailingStopLine(drawingContext);

		RenderMarkers(drawingContext);
	}

    private void RenderTrailingStopLine(IDrawingContext drawingContext)
    {
		if (!ShowStopLine)
		{
			return;
		}

		var firstRenderIndex = Math.Max(0, Chart.FirstVisibleBarIndex - 1);
		var lastRenderIndex = Math.Min(StopDots.Count, Chart.LastVisibleBarIndex + 1);

		for (var renderIndex = firstRenderIndex; renderIndex < lastRenderIndex - 1; renderIndex++)
		{
			var startIndex = renderIndex;
			var startTrend = _trends[startIndex];
			var startColor = StopDots.Colors[startIndex];
			var startPoint = StopDots.GetPoint(startIndex);

			var endIndex = renderIndex + 1;
			var endTrend = _trends[endIndex];
			var endPoint = StopDots.GetPoint(endIndex);

			var startApiPoint = this.ToApiPoint(startPoint);
			var endApiPoint = this.ToApiPoint(endPoint);

			if (startTrend.EnumEquals(endTrend))
			{
				drawingContext.DrawLine(startApiPoint, endApiPoint, startColor, StopLineThickness, StopLineStyle);
			}
		}
    }

	private void RenderMarkers(IDrawingContext drawingContext)
	{
		if (!ShowMarkers)
		{
			return;
		}

		var boundary = this.GetVisibleBoundary();
		var markerPoints = _markers.GetVisibleDrawingParts(boundary)
			.Select(drawingPart => drawingPart.Key);

		foreach (var markerPoint in markerPoints)
		{
			var point = this.ToApiPoint(markerPoint);
			var trend = _trends[markerPoint.BarIndex];
			
			var markerColor = trend.Map(BearishColor, BullishColor);
			var markerText = trend.Map("5", "6");
			var markerTextSize = drawingContext.MeasureText(markerText, _markerFont);

			point.X -= markerTextSize.Width / 2.0;
			point.Y -= markerTextSize.Height / 2.0;

			drawingContext.DrawText(point, markerText, markerColor, _markerFont);
		}
	}
}
