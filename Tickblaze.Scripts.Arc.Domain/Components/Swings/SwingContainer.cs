using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Collections.Generic.EqualityComparer<Tickblaze.Scripts.Arc.Domain.StrictTrend>;

namespace Tickblaze.Scripts.Arc.Domain;

public class SwingContainer : ComponentContainer<Point, SwingLine>
{
	private int _currentIndex = 1;
	private StrictTrend _currentTrend;
	private StrictTrend? _previousTrend;
	private readonly ComponentContainer<Point, SwingLine> _pendingSwings = [];

	public required int SwingStrength { get; init; }

	public required BarSeries BarSeries
	{
		get;
		init
		{
			field = value;

			SwingDeviation ??= field.Map(bar => 0.0d);
			DoubleTopBottomDeviation ??= field.Map(bar => 0.0d);
		}
	}

	[NotNull]
	public ISeries<double>? SwingDeviation { get; init; }

	[NotNull]
	public ISeries<double>? DoubleTopBottomDeviation { get; init; }

	public required SwingCalculationMode CalculationMode { get; init; }

	private int BarOffset => CalculationMode is SwingCalculationMode.CurrentBar ? 0 : 1;

	private double GetTrendPrice(StrictTrend trend, int barIndex)
	{
		return trend.Map(BarSeries.High[barIndex], BarSeries.Low[barIndex]);
	}

	protected virtual double GetLookbackHigh(int barIndex)
	{
		var endBarIndex = barIndex;
		var lookbackHigh = double.MinValue;
		var startBarIndex = Math.Max(endBarIndex - SwingStrength, 0);

		for (var index = startBarIndex; index < endBarIndex; index++)
		{
			lookbackHigh = Math.Max(lookbackHigh, BarSeries.High[index]);
		}

		if (!IsEmpty && LastComponent is { Trend: StrictTrend.Down } lastSwing)
		{
			var lastLowPrice = lastSwing.EndPoint.Price;
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
			lookbackLow = Math.Min(lookbackLow, BarSeries.Low[index]);
		}

		if (!IsEmpty && LastComponent is { Trend: StrictTrend.Up } lastSwing)
		{
			var lastHighPrice = lastSwing.EndPoint.Price;
			var swingDeviation = SwingDeviation[barIndex];

			lookbackLow = Math.Min(lookbackLow, lastHighPrice - swingDeviation);
		}

		return lookbackLow;
	}

	public override IEnumerable<SwingLine> GetVisibleComponents(Rectangle visibleRectangle)
	{
		var visibleSwings = base.GetVisibleComponents(visibleRectangle);

		var visiblePendingSwings = _pendingSwings
			.GetVisibleComponents(visibleRectangle)
			.ToDictionary(swing => swing.StartPoint);

		foreach (var visibleSwing in visibleSwings)
		{
			if (_pendingSwings.TryGetComponent(visibleSwing.StartPoint, out var pendingVisibleSwing))
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
		var isUpTrend = BarSeries.High[barIndex] > BarSeries.High[barIndex - 1];
		var isDownTrend = BarSeries.Low[barIndex] < BarSeries.Low[barIndex - 1];

		if (!isUpTrend && !isDownTrend)
		{
			return false;
		}

		_currentTrend = isUpTrend ? StrictTrend.Up : StrictTrend.Down;

		var oppositeTrend = _currentTrend.GetOppositeTrend();
		var currentEndPrice = GetTrendPrice(_currentTrend, barIndex);
		var previousEndPrice = GetTrendPrice(oppositeTrend, barIndex);
		var previousStartPrice = GetTrendPrice(_currentTrend, 0);

		if (barIndex >= 2)
		{
			var previousSwing = new SwingLine
			{
				Trend = oppositeTrend,
				EndPoint = new(barIndex, previousEndPrice),
				StartPoint = new(0, previousStartPrice),
			};

			_pendingSwings.Upsert(previousSwing);
		}

		var currentSwing = new SwingLine
		{
			Trend = _currentTrend,
			EndPoint = new(barIndex, currentEndPrice),
			StartPoint = new(barIndex, previousEndPrice),
		};

		_pendingSwings.Upsert(currentSwing);

		return true;
	}

	private void TryUpsertPendingSwings(int barIndex)
	{
		if (_currentIndex.Equals(barIndex))
		{
			_pendingSwings.Clear();

			return;
		}

		_currentIndex = barIndex;

		if (IsEmpty && _pendingSwings.IsEmpty)
		{
			return;
		}

		_previousTrend = _currentTrend;

		foreach (var pendingSwing in _pendingSwings)
		{
			if (!IsEmpty)
			{
				var lastSwing = LastComponent;

				lastSwing.Label = GetLastSwingLabel();
			}

			Upsert(pendingSwing);
		}

		_pendingSwings.Clear();
	}

	public void CalculateSwings(int barIndex)
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

			return;
		}

		var currentLow = BarSeries.Low[barIndex];
		var currentHigh = BarSeries.High[barIndex];
		var currentClose = BarSeries.Close[barIndex];

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
	}

	private void CalculateSwings(int barIndex, bool isOutsideBar)
	{
		if (_previousTrend is null)
		{
			throw new InvalidOperationException(nameof(CalculateSwings));
		}

		var isTrendBreak = !Default.Equals(_previousTrend.Value, _currentTrend);

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

	private void UpdateSwing(int barIndex)
	{
		var currentSwing = LastComponent;
		var currentTrend = currentSwing.Trend;
		var currentEndPrice = currentSwing.EndPoint.Price;
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);

		if (currentTrend is StrictTrend.Up && currentEndPrice <= currentTrendPrice
			|| currentTrend is StrictTrend.Down && currentEndPrice >= currentTrendPrice)
		{
			var updatedSwing = new SwingLine
			{
				Trend = currentTrend,
				StartPoint = currentSwing.StartPoint,
				EndPoint = new(barIndex, currentTrendPrice),
			};

			_pendingSwings.Upsert(updatedSwing);
		}
	}

	private void AlternateSwing(int barIndex)
	{
		var previousSwing = _pendingSwings.IsEmpty
			? LastComponent : _pendingSwings.LastComponent;
		var previousTrend = previousSwing.Trend;
		var previousStartPoint = previousSwing.EndPoint;
		var currentTrend = previousTrend.GetOppositeTrend();
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);

		var currentSwing = new SwingLine
		{
			Trend = currentTrend,
			StartPoint = previousStartPoint,
			EndPoint = new(barIndex, currentTrendPrice),
		};

		_pendingSwings.Upsert(currentSwing);
	}

	private SwingLabel GetLastSwingLabel()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(nameof(GetLastSwingLabel));
		}

		var lastSwing = LastComponent;
		var lastTrend = lastSwing.Trend;

		if (Count is 1)
		{
			return lastTrend.Map(SwingLabel.HigherHigh, SwingLabel.LowerLow);
		}

		var lastEndPoint = lastSwing.EndPoint;
		var lastPrice = lastEndPoint.Price;
		var lastBarIndex = lastEndPoint.BarIndex;
		var lastDoubleTopBottomDeviation = DoubleTopBottomDeviation[lastBarIndex];
		
		var secondLastSwing = GetComponentAt(^2);
		var secondLastPrice = secondLastSwing.StartPoint.Price;
		
		var priceDelta = lastPrice - secondLastPrice;

		if (Math.Abs(priceDelta) <= lastDoubleTopBottomDeviation)
		{
			return lastTrend.Map(SwingLabel.DoubleTop, SwingLabel.DoubleBottom);
		}

		return priceDelta.CompareTo(lastDoubleTopBottomDeviation) switch
		{
			> 0 when lastTrend is StrictTrend.Up => SwingLabel.HigherHigh,
			< 0 when lastTrend is StrictTrend.Up => SwingLabel.LowerHigh,
			> 0 when lastTrend is StrictTrend.Down => SwingLabel.HigherLow,
			< 0 when lastTrend is StrictTrend.Down => SwingLabel.LowerLow,
			_ => throw new UnreachableException(),
		};
	}
}
