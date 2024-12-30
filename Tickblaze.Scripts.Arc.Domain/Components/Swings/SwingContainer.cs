using System.Diagnostics;
using static System.Collections.Generic.EqualityComparer<Tickblaze.Scripts.Arc.Domain.StrictTrend>;

namespace Tickblaze.Scripts.Arc.Domain;

public sealed class SwingContainer : ComponentContainer<SwingSegment>
{
	private int _currentIndex = 1;
	private StrictTrend _currentTrend;
	private StrictTrend? _previousTrend;
	private readonly ComponentContainer<SwingSegment> _pendingSwings = [];

	public required int SwingStrength { get; init; }

	public required BarSeries BarSeries { get; init; }

	public required SwingCalculationMode CalculationMode { get; init; }

	private int BarOffset => CalculationMode is SwingCalculationMode.CurrentBar ? 0 : 1;

	private double GetTrendPrice(StrictTrend trend, int barIndex)
	{
		return trend.Map(BarSeries.High[barIndex], BarSeries.Low[barIndex]);
	}

	private double GetLookbackHigh(int barIndex)
	{
		var toBarIndex = barIndex;
		var lookbackHigh = double.MinValue;
		var fromBarIndex = Math.Max(toBarIndex - SwingStrength, 0);

		for (var index = fromBarIndex; index < toBarIndex; index++)
		{
			lookbackHigh = Math.Max(lookbackHigh, BarSeries.High[index]);
		}

		return lookbackHigh;
	}

	private double GetLookbackLow(int barIndex)
	{
		var toBarIndex = barIndex;
		var lookbackLow = double.MaxValue;
		var fromBarIndex = Math.Max(toBarIndex - SwingStrength, 0);

		for (var index = fromBarIndex; index < toBarIndex; index++)
		{
			lookbackLow = Math.Min(lookbackLow, BarSeries.Low[index]);
		}

		return lookbackLow;
	}

	public override IEnumerable<SwingSegment> GetVisibleComponents(Rectangle visibleRectangle)
	{
		var visibleSwings = base.GetVisibleComponents(visibleRectangle);

		var pendingVisibleSwings = _pendingSwings
			.GetVisibleComponents(visibleRectangle)
			.ToSortedDictionary(swing => swing.FromBarIndex);

		foreach (var visibleSwing in visibleSwings)
		{
			if (_pendingSwings.TryGetComponent(visibleSwing.FromBarIndex, out var pendingVisibleSwing))
			{
				pendingVisibleSwings.Remove(visibleSwing.FromBarIndex);

				yield return pendingVisibleSwing;
			}
			else
			{
				yield return visibleSwing;
			}
		}

		foreach (var visibleSwing in pendingVisibleSwings.Values)
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
		var currentToPrice = GetTrendPrice(_currentTrend, barIndex);
		var previousToPrice = GetTrendPrice(oppositeTrend, barIndex);
		var previousFromPrice = GetTrendPrice(_currentTrend, 0);

		if (barIndex >= 2)
		{
			var previousSwing = new SwingSegment
			{
				Trend = oppositeTrend,
				ToPoint = new(barIndex, previousToPrice),
				FromPoint = new(0, previousFromPrice),
			};

			_pendingSwings.Upsert(previousSwing);
		}

		var currentSwing = new SwingSegment
		{
			Trend = _currentTrend,
			ToPoint = new(barIndex, currentToPrice),
			FromPoint = new(barIndex, previousToPrice),
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

		_previousTrend = _currentTrend;

		foreach (var pendingSwing in _pendingSwings)
		{
			if (!IsEmpty)
			{
				UpdateLastSwingLabel();
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
		var currentToPrice = currentSwing.ToPoint.Price;
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);

		if (currentTrend is StrictTrend.Up && currentToPrice <= currentTrendPrice
			|| currentTrend is StrictTrend.Down && currentToPrice >= currentTrendPrice)
		{
			var updatedSwing = new SwingSegment
			{
				Trend = currentTrend,
				FromPoint = currentSwing.FromPoint,
				ToPoint = new(barIndex, currentTrendPrice),
			};

			_pendingSwings.Upsert(updatedSwing);
		}
	}

	private void AlternateSwing(int barIndex)
	{
		var previousSwing = LastComponent;
		var previousTrend = previousSwing.Trend;
		var previousToPoint = previousSwing.ToPoint;
		var currentTrend = previousTrend.GetOppositeTrend();
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);

		var currentSwing = new SwingSegment
		{
			Trend = currentTrend,
			FromPoint = previousToPoint,
			ToPoint = new(barIndex, currentTrendPrice),
		};

		_pendingSwings.Upsert(currentSwing);
	}

	private SwingLabel UpdateLastSwingLabel()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException(nameof(UpdateLastSwingLabel));
		}

		var lastSwing = LastComponent;

		if (Count is 1)
		{
			return lastSwing.Trend.Map(SwingLabel.HigherHigh, SwingLabel.LowerLow);
		}

		var secondLastSwing = GetComponentAt(^2);

		var lastTrend = lastSwing.Trend;
		var lastPrice = lastSwing.ToPoint.Price;
		var secondLastPrice = secondLastSwing.FromPoint.Price;

		return lastPrice.CompareTo(secondLastPrice) switch
		{
			> 0 when lastTrend is StrictTrend.Up => SwingLabel.HigherHigh,
			< 0 when lastTrend is StrictTrend.Up => SwingLabel.LowerHigh,
			0 when lastTrend is StrictTrend.Up => SwingLabel.DoubleTop,
			> 0 when lastTrend is StrictTrend.Down => SwingLabel.HigherLow,
			< 0 when lastTrend is StrictTrend.Down => SwingLabel.LowerLow,
			0 when lastTrend is StrictTrend.Down => SwingLabel.DoubleBottom,
			_ => throw new UnreachableException(),
		};
	}
}
