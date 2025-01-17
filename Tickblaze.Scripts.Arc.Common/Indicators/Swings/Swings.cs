using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Swings : ChildIndicator
{
    private int _currentIndex;
    
	private StrictTrend _currentTrend;
    
	private StrictTrend? _previousTrend;

	private readonly DrawingPartDictionary<Point, SwingLine> _pendingSwings = [];

    public required bool IsSwingEnabled { get; set; }

    public required int SwingStrength { get; init; }

    public bool ShowDots { get; init; }

    public int DotSize { get; init; }

    public bool ShowLabels { get; set; }

    public required Font LabelFont { get; init; }

    public Color UpLabelColor { get; init; }

    public Color DownLabelColor { get; init; }

	public bool IsDtbLabelColorEnabled { get; init; }
    
	public Color DtbLabelColor { get; init; }

    public bool ShowLines { get; set; }

    public Color UpLineColor { get; init; }

    public Color DownLineColor { get; init; }

    public int LineThickness { get; init; }

    public LineStyle LineStyle { get; init; }

    public required SwingCalculationMode CalculationMode { get; init; }

    private SwingLine LastSwing => _swings.LastDrawingPart;

    private int BarOffset => CalculationMode is SwingCalculationMode.CurrentBar ? 0 : 1;

	private readonly DrawingPartDictionary<Point, SwingLine> _swings = [];

    public IReadOnlyList<SwingLine> SwingList => _swings;

	private readonly List<Trend> _trendBiases = [];

    public ISeries<Trend> TrendBiases => field ??= _trendBiases.AsSeries();

    private ISeries<double> _swingDeviation = default!;

    public ISeries<double> SwingDeviation
    {
        get => _swingDeviation;
        init => _swingDeviation = value;
    }

    private ISeries<double> _swingDtbDeviation = default!;

    /// <summary>
    /// Swing Double Top/Bottom Deviation.
    /// </summary>
    public ISeries<double> SwingDtbDeviation
    {
        get => _swingDtbDeviation;
        init => _swingDtbDeviation = value;
    }

    private double GetTrendPrice(StrictTrend trend, int barIndex)
    {
        return trend.Map(Bars.High[barIndex], Bars.Low[barIndex]);
    }

    protected virtual double GetLookbackHigh(int barIndex)
    {
        var endBarIndex = barIndex;
        var lookbackHigh = double.MinValue;
        var startBarIndex = Math.Max(endBarIndex - SwingStrength, 0);

        for (var index = startBarIndex; index < endBarIndex; index++)
        {
            lookbackHigh = Math.Max(lookbackHigh, Bars.High[index]);
        }

        if (_swings is { IsEmpty: false, LastDrawingPart.Trend: StrictTrend.Down })
        {
            var lastLowPrice = LastSwing.EndPoint.Price;
            var swingDeviation = SwingDeviation[barIndex];

            lookbackHigh = Math.Max(lookbackHigh, lastLowPrice + swingDeviation);
        }

        return lookbackHigh;
    }

    protected virtual double GetLookbackLow(int barIndex)
    {
        var endBarIndex = barIndex;
        var lookbackLow = double.MaxValue;
        var startBarIndex = Math.Max(endBarIndex - SwingStrength, 0);

        for (var index = startBarIndex; index < endBarIndex; index++)
        {
            lookbackLow = Math.Min(lookbackLow, Bars.Low[index]);
        }

        if (_swings is { IsEmpty: false, LastDrawingPart.Trend: StrictTrend.Up })
        {
            var lastHighPrice = LastSwing.EndPoint.Price;
            var swingDeviation = SwingDeviation[barIndex];

            lookbackLow = Math.Min(lookbackLow, lastHighPrice - swingDeviation);
        }

        return lookbackLow;
    }

    private Trend GetTrendBias(int barIndex, DrawingPartDictionary<Point, SwingLine> lastSwings)
    {
        if (barIndex is 0 || lastSwings is not [.., var thirdLastSwing, var secondLastSwing, var lastSwing])
        {
            return Trend.None;
        }

        var lastLabel = lastSwing.Label;
        var isForthLastLowerLow = false;
        var isForthLastHigherHigh = false;

        if (lastSwings.Count >= 4)
        {
            var forthLastSwing = lastSwings.GetDrawingPartAt(^4);
            var forthLastLabel = forthLastSwing.Label;

            isForthLastLowerLow = forthLastLabel.IsLowerLow;
            isForthLastHigherHigh = forthLastLabel.IsHigherHigh;
        }

        var previousTrendBias = _trendBiases[barIndex - 1];
        var currentTrendBias = previousTrendBias;

        if (lastLabel.IsHigherHigh
            && !secondLastSwing.Label.IsLowerLow
            && (thirdLastSwing.Label.IsHigherHigh || !isForthLastLowerLow))
        {
            currentTrendBias = Trend.Up;
        }

        if (lastLabel.IsLowerLow
            && !secondLastSwing.Label.IsHigherHigh
            && (thirdLastSwing.Label.IsLowerLow || !isForthLastHigherHigh))
        {
            currentTrendBias = Trend.Down;
        }

        if (lastLabel.IsLowerLow && previousTrendBias is Trend.Up
            || lastLabel.IsHigherHigh && previousTrendBias is Trend.Down)
        {
            currentTrendBias = Trend.None;
        }

        return currentTrendBias;
    }

	private SwingLabel GetIncomingSwingLabel(StrictTrend incomingTrend, int incomingBarIndex, double incomingPrice)
	{
		var lastSwings = GetLastSwings(2);
		var lastSwing = lastSwings.GetLastOrDefault();

		if (Nullable.Equals(incomingTrend, lastSwing?.Trend))
		{
			lastSwing = lastSwings.GetAtOrDefault(^2, default);
		}

		if (lastSwing is null)
		{
			return incomingTrend.Map(SwingLabel.HigherHigh, SwingLabel.LowerLow);
		}

		var incomingDtbDeviation = SwingDtbDeviation[incomingBarIndex];

		var priceDelta = incomingPrice - lastSwing.StartPrice;

		if (Math.Abs(priceDelta) <= incomingDtbDeviation)
		{
			return incomingTrend.Map(SwingLabel.DoubleTop, SwingLabel.DoubleBottom);
		}

		return priceDelta.CompareTo(0) switch
		{
			> 0 when incomingTrend is StrictTrend.Up => SwingLabel.HigherHigh,
			< 0 when incomingTrend is StrictTrend.Up => SwingLabel.LowerHigh,
			> 0 when incomingTrend is StrictTrend.Down => SwingLabel.HigherLow,
			< 0 when incomingTrend is StrictTrend.Down => SwingLabel.LowerLow,
			_ => throw new UnreachableException(),
		};
	}

	private DrawingPartDictionary<Point, SwingLine> GetLastSwings(int maxCount)
	{
		var lastSwings = new DrawingPartDictionary<Point, SwingLine>();

		var startSwingIndex = Math.Max(0, _swings.Count - maxCount);

		for (var swingIndex = startSwingIndex; swingIndex < _swings.Count; swingIndex++)
		{
			var swing = _swings.GetDrawingPartAt(swingIndex);

			lastSwings.AddOrUpdate(swing);
		}

		foreach (var pendingSwing in _pendingSwings)
		{
			lastSwings.AddOrUpdate(pendingSwing);
		}

		while (!lastSwings.IsEmpty && lastSwings.Count > maxCount)
		{
			lastSwings.RemoveAt(0);
		}

		return lastSwings;
	}

	private IEnumerable<SwingLine> GetVisibleSwings(Rectangle visibleRectangle)
    {
        var visibleSwings = _swings.GetVisibleDrawingParts(visibleRectangle);

        var visiblePendingSwings = _pendingSwings
            .GetVisibleDrawingParts(visibleRectangle)
            .ToDictionary(swing => swing.StartPoint);

        foreach (var visibleSwing in visibleSwings)
        {
            if (_pendingSwings.TryGetDrawingPart(visibleSwing.StartPoint, out var pendingVisibleSwing))
            {
                visiblePendingSwings.Remove(visibleSwing.StartPoint);

                yield return pendingVisibleSwing;
            }
            else
            {
                yield return visibleSwing;
            }
        }

        foreach (var visibleSwing in visiblePendingSwings.Values)
        {
            yield return visibleSwing;
        }
    }

    private bool TryInitializeSwings(int barIndex)
    {
        var isUpTrend = Bars.High[barIndex] > Bars.High[barIndex - 1];
        var isDownTrend = Bars.Low[barIndex] < Bars.Low[barIndex - 1];

        if (!isUpTrend && !isDownTrend)
        {
            return false;
        }

        _currentTrend = isUpTrend ? StrictTrend.Up : StrictTrend.Down;

        var oppositeTrend = _currentTrend.GetOppositeTrend();
        
		var currentEndPrice = GetTrendPrice(_currentTrend, barIndex);
		var currentLabel = GetIncomingSwingLabel(_currentTrend, barIndex, currentEndPrice);

        var previousEndPrice = GetTrendPrice(oppositeTrend, barIndex);
        var previousStartPrice = GetTrendPrice(_currentTrend, 0);
		var previousLabel = GetIncomingSwingLabel(oppositeTrend, barIndex, previousEndPrice);

		if (barIndex >= 2)
        {
            var previousSwing = new SwingLine
            {
                Label = previousLabel,
                Trend = oppositeTrend,
                EndPoint = new(barIndex, previousEndPrice),
                StartPoint = new(0, previousStartPrice),
			};

            _pendingSwings.AddOrUpdate(previousSwing);
        }

        var currentSwing = new SwingLine
        {
            Label = currentLabel,
            Trend = _currentTrend,
            EndPoint = new(barIndex, currentEndPrice),
            StartPoint = new(barIndex, previousEndPrice),
		};

        _pendingSwings.AddOrUpdate(currentSwing);

        return true;
    }

    protected override void Initialize()
    {
		_currentIndex = default;
		_currentTrend = default;
		_previousTrend = default;

		_swings.Clear();
		_trendBiases.Clear();
		_pendingSwings.Clear();

        _swingDeviation ??= Bars.Map(bar => 0.0d);
		_swingDtbDeviation ??= Bars.Map(bar => 0.0d);
    }

	public void Reinitialize()
	{
		Initialize();

		Calculate();
	}

    private void TryUpsertPendingSwings(int barIndex)
    {
        if (_currentIndex.Equals(barIndex))
        {
            _pendingSwings.Clear();

            return;
        }

        _currentIndex = barIndex;

        if (_swings.IsEmpty && _pendingSwings.IsEmpty)
        {
            return;
        }

        _previousTrend = _currentTrend;

        foreach (var pendingSwing in _pendingSwings)
        {
            _swings.AddOrUpdate(pendingSwing);
        }

        _pendingSwings.Clear();
    }

    protected override void Calculate(int barIndex)
    {
        barIndex -= BarOffset;

        if (barIndex <= 0)
        {
            return;
        }

        TryUpsertPendingSwings(barIndex);

		if (_previousTrend is null)
        {
            TryInitializeSwings(barIndex);

			CalculateTrendBiases(barIndex);

			return;
        }

        var currentLow = Bars.Low[barIndex];
        var currentHigh = Bars.High[barIndex];
        var currentClose = Bars.Close[barIndex];

        var lookbackLow = GetLookbackLow(barIndex);
        var lookbackHigh = GetLookbackHigh(barIndex);

        var isUpTrend = currentHigh > lookbackHigh;
        var isDownTrend = currentLow < lookbackLow;
        var isOutsideBar = isUpTrend && isDownTrend;

        if (isOutsideBar)
        {
            _currentTrend = (currentHigh - currentClose).CompareTo(currentClose - currentLow) switch
            {
                > 0 => StrictTrend.Up,
                < 0 => StrictTrend.Down,
                0 => _previousTrend.Value,
            };
        }
        else if (isUpTrend)
        {
            _currentTrend = StrictTrend.Up;
        }
        else if (isDownTrend)
        {
            _currentTrend = StrictTrend.Down;
        }
        else
        {
            _currentTrend = _previousTrend.Value;
        }

        CalculateSwings(barIndex, isOutsideBar);

		CalculateTrendBiases(barIndex);
	}

    private void CalculateSwings(int barIndex, bool isOutsideBar)
    {
        if (_previousTrend is null)
        {
            throw new InvalidOperationException(nameof(CalculateSwings));
        }

        var isTrendBreak = !_currentTrend.EnumEquals(_previousTrend.Value);

        if (isOutsideBar)
        {
            if (isTrendBreak)
            {
                UpdateSwing(barIndex);
            }
            else
            {
                AlternateSwing(barIndex);
            }

            AlternateSwing(barIndex);
        }
        else if (isTrendBreak)
        {
            AlternateSwing(barIndex);
        }
        else
        {
            UpdateSwing(barIndex);
        }
    }

    private void CalculateTrendBiases(int barIndex)
    {
        var lastSwings = GetLastSwings(4);
		var trendBias = GetTrendBias(barIndex, lastSwings);

		_trendBiases.BackfillAddOrUpdate(barIndex, trendBias, Trend.None);
    }

    private void UpdateSwing(int barIndex)
    {
        var currentSwing = _swings.LastDrawingPart;
        var currentTrend = currentSwing.Trend;
        var currentEndPrice = currentSwing.EndPoint.Price;
        var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);
        var currentLabel = GetIncomingSwingLabel(currentTrend, barIndex, currentTrendPrice);

        if (currentTrend is StrictTrend.Up && currentEndPrice <= currentTrendPrice
            || currentTrend is StrictTrend.Down && currentEndPrice >= currentTrendPrice)
        {
            var updatedSwing = new SwingLine
            {
                Trend = currentTrend,
                Label = currentLabel,
                StartPoint = currentSwing.StartPoint,
                EndPoint = new(barIndex, currentTrendPrice),
            };

            _pendingSwings.AddOrUpdate(updatedSwing);
        }
    }

    private void AlternateSwing(int barIndex)
    {
        var previousSwing = _pendingSwings.IsEmpty ? LastSwing : _pendingSwings.LastDrawingPart;
        var previousTrend = previousSwing.Trend;
        var previousStartPoint = previousSwing.EndPoint;
        var currentTrend = previousTrend.GetOppositeTrend();
        var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);
        var currentLabel = GetIncomingSwingLabel(currentTrend, barIndex, currentTrendPrice);

        var currentSwing = new SwingLine
        {
            Trend = currentTrend,
            Label = currentLabel,
            StartPoint = previousStartPoint,
            EndPoint = new(barIndex, currentTrendPrice),
        };

        _pendingSwings.AddOrUpdate(currentSwing);
    }

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (Chart is null || ChartScale is null || RenderTarget is null)
		{
			throw new InvalidOperationException(nameof(OnRender));
		}

		if (!IsSwingEnabled || !ShowLines && !ShowDots && !ShowLabels)
        {
            return;
        }

        var previousDotColor = Color.Transparent;
        var boundary = RenderTarget.GetVisibleBoundary();
        var swings = GetVisibleSwings(boundary);

        foreach (var swing in swings)
        {
            var trend = swing.Trend;
            var swingLabel = swing.Label;
            var lineColor = trend.Map(UpLineColor, DownLineColor);
            var endPoint = RenderTarget.GetApiPoint(swing.EndPoint);
            var startPoint = RenderTarget.GetApiPoint(swing.StartPoint);
            var isDtb = swingLabel.IsDoubleTop || swingLabel.IsDoubleBottom;

            if (ShowLines)
            {
                drawingContext.DrawLine(startPoint, endPoint, lineColor, LineThickness);
            }

            if (ShowDots)
            {
                drawingContext.DrawEllipse(startPoint, DotSize / 2.0, DotSize / 2.0, previousDotColor);

                previousDotColor = isDtb ? DtbLabelColor : lineColor;
            }

            if (ShowLabels)
            {
                var label = swing.Label.ShortName;
                var labelSize = drawingContext.MeasureText(label, LabelFont);
                var labelVerticalOffset = swing.Trend.Map(-VerticalMargin - labelSize.Height, VerticalMargin);
                var labelHorizontalOffset = -labelSize.Width / 2.0;
                var labelColor = trend switch
                {
                    _ when isDtb && IsDtbLabelColorEnabled => DtbLabelColor,
                    StrictTrend.Up => UpLabelColor,
                    StrictTrend.Down => DownLabelColor,
                    _ => throw new UnreachableException(),
                };

                endPoint.Y += labelVerticalOffset;
                endPoint.X += labelHorizontalOffset;

                drawingContext.DrawText(endPoint, label, labelColor, LabelFont);
            }
        }
    }
}
