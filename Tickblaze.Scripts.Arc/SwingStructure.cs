﻿using System.Diagnostics;
using Tickblaze.Scripts.Arc.Domain;
using static System.Collections.Generic.EqualityComparer<Tickblaze.Scripts.Arc.Domain.StrictTrend>;

namespace Tickblaze.Scripts.Arc;

public partial class SwingStructure : Indicator
{
	public SwingStructure()
	{
	}

	private int _barOffset;
	private StrictTrend _currentTrend;
	private StrictTrend? _previousTrend;

	private readonly ComponentContainer<SwingSegment> _swingContainer = new();

	[Parameter("Calculation Source", Description = "Whether to calculate the structure by current or closed bar highs and lows")]
	public CalculationSource CalculationSourceValue { get; set; }

	[NumericRange(MinValue = 1, MaxValue = 200)]
	[Parameter("Swing strength", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 1;

	[Parameter("Show Lines", Description = "Show structure trend lines")]
	public bool ShowSwingLines { get; set; } = true;

	[Parameter("Up-trend line color", Description = "Line color for up-trending structure")]
	public Color UpLineColor { get; set; } = Color.FromDrawingColor(DrawingColor.DarkGreen);

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

	private double GetTrendPrice(StrictTrend trend, int barIndex)
	{
		return trend switch
		{
			StrictTrend.Up => Bars.High[barIndex],
			StrictTrend.Down => Bars.Low[barIndex],
			_ => throw new UnreachableException(),
		};
	}

	private Color GetTrendColor(StrictTrend trend)
    {
        return trend switch
        {
			StrictTrend.Up => UpLineColor,
			StrictTrend.Down => DownLineColor,
			_ => throw new UnreachableException(),
		};
	}

	private static SwingLabel GetInitialSwingLabel(StrictTrend trend)
	{
		return trend switch
		{
			StrictTrend.Up => SwingLabel.LowerLow,
			StrictTrend.Down => SwingLabel.HigherHigh,
			_ => throw new UnreachableException(),
		};
	}

	private SwingLabel GetSwingLabel(double price)
	{
		if (_swingContainer.IsEmpty)
		{
			throw new InvalidOperationException(nameof(GetSwingLabel));
		}

		if (_swingContainer.Count is 1)
		{
			var lastSwing = _swingContainer.LastComponent;

			var oppositeTrend = lastSwing.Trend.GetOppositeTrend();

			return GetInitialSwingLabel(oppositeTrend);
		}

		var secondLastSwing = _swingContainer.GetComponentAt(^2);

		var secondLastTrend = secondLastSwing.Trend;
		var secondLastPrice = secondLastSwing.FromPoint.Price;

		return price.CompareTo(secondLastPrice) switch
		{
			> 0 when secondLastTrend is StrictTrend.Up => SwingLabel.HigherLow,
			< 0 when secondLastTrend is StrictTrend.Up => SwingLabel.LowerLow,
			0 when secondLastTrend is StrictTrend.Up => SwingLabel.DoubleBottom,
			> 0 when secondLastTrend is StrictTrend.Down => SwingLabel.HigherHigh,
			< 0 when secondLastTrend is StrictTrend.Down => SwingLabel.LowerHigh,
			0 when secondLastTrend is StrictTrend.Down => SwingLabel.DoubleTop,
			_ => throw new UnreachableException(),
		};
	}

	private double GetLookbackHigh(int barIndex)
	{
		var toBarIndex = barIndex;
		var lookbackHigh = double.MinValue;
		var fromBarIndex = Math.Max(toBarIndex - SwingStrength, 0);

		for (var index = fromBarIndex; index < toBarIndex; index++)
		{
			lookbackHigh = Math.Max(lookbackHigh, Bars.High[index]);
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
			lookbackLow = Math.Min(lookbackLow, Bars.Low[index]);
		}

		return lookbackLow;
	}

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
		_swingContainer.Clear();

		_currentTrend = default;
		_previousTrend = default;

		_barOffset = CalculationSourceValue switch
		{
			CalculationSource.CurrentBar => 0,
			CalculationSource.ClosedBar => 1,
			_ => throw new UnreachableException()
		};
    }

    protected override void Calculate(int index)
    {
        if (index <= _barOffset)
        {
            return;
        }

		var barIndex = index - _barOffset;
        
		if (_previousTrend is null)
		{
			TryInitializeStructure(barIndex);

			return;
		}

		// Todo: approve barIndex with Jon [differ from NT].
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
			_currentTrend = (currentHigh - currentClose).CompareTo(currentClose - currentLow)
				switch
			{
				> 0 => StrictTrend.Up,
				< 0 => StrictTrend.Down,
				0 => _previousTrend.Value,
			};
		}
		if (isUpTrend)
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

		_previousTrend = _currentTrend;
    }

	private bool TryInitializeStructure(int barIndex)
	{
		var isUpTrend = Bars.High[barIndex] > Bars.High[barIndex - 1];
		var isDownTrend = Bars.Low[barIndex] < Bars.Low[barIndex - 1];

		if (!isUpTrend && !isDownTrend)
		{
			return false;
		}

		_currentTrend = isUpTrend ? StrictTrend.Up : StrictTrend.Down;

		var oppositeTrend = _currentTrend.GetOppositeTrend();
		var currentSwingLabel = GetInitialSwingLabel(_currentTrend);
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

			_swingContainer.Upsert(previousSwing);
		}

		var currentSwing = new SwingSegment
		{
			Trend = _currentTrend,
			FromLabel = currentSwingLabel,
			ToPoint = new(barIndex, currentToPrice),
			FromPoint = new(barIndex, previousToPrice),
		};

		_swingContainer.Upsert(currentSwing);

		_previousTrend = _currentTrend;

		return true;
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
		var currentSwing = _swingContainer.LastComponent;

		var currentTrend = currentSwing.Trend;
		var currentToPrice = currentSwing.ToPoint.Price;
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);

		if (currentTrend is StrictTrend.Up && currentToPrice <= currentTrendPrice
			|| currentTrend is StrictTrend.Down && currentToPrice >= currentTrendPrice)
		{
			currentSwing.ToPoint = new(barIndex, currentTrendPrice);
		}
	}

	private void AlternateSwing(int barIndex)
	{
		var previousSwing = _swingContainer.LastComponent;

		var previousTrend = previousSwing.Trend;
		var currentTrend = previousTrend.GetOppositeTrend();
		var currentTrendPrice = GetTrendPrice(currentTrend, barIndex);
		var currentSwingLabel = GetSwingLabel(currentTrendPrice);

		var currentSwing = new SwingSegment
		{
			Trend = currentTrend,
			FromLabel = currentSwingLabel,
			FromPoint = previousSwing.ToPoint,
			ToPoint = new(barIndex, currentTrendPrice),
		};

		_swingContainer.Upsert(currentSwing);
	}

    public override void OnRender(IDrawingContext context)
    {
		if (!ShowSwingLines)
		{
			return;
		}

		var visibleRectangle = this.GetVisibleRectangle();
		var swings = _swingContainer.GetVisibleComponents(visibleRectangle);

		foreach (var swing in swings)
		{
			var color = GetTrendColor(swing.Trend);
			var toPoint = this.GetPoint(swing.ToPoint);
			var fromPoint = this.GetPoint(swing.FromPoint);
			var oppositeTrend = swing.Trend.GetOppositeTrend();
			var oppositeColor = GetTrendColor(oppositeTrend);

			context.DrawLine(fromPoint, toPoint, color, LineThickness);

			if (ShowSwingLabels)
			{
				var label = swing.FromLabel.ShortName;
				var labelSize = context.MeasureText(label, LabelFont);
				var labelOffset = swing.Trend switch
				{
					StrictTrend.Up => 3.0f,
					StrictTrend.Down => - 3.0f - labelSize.Height,
					_ => throw new UnreachableException(),
				};

				fromPoint.Y += labelOffset;

				context.DrawText(fromPoint, label, oppositeColor, LabelFont);
			}
		}
    }
}
