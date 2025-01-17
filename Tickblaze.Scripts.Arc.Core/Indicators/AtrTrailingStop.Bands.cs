﻿using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class AtrTrailingStop
{
	[AllowNull]
	private PlotSeries _bandUpper1;

	private ISeries<ApiPoint> BandUpper1 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private PlotSeries _bandLower1;

	private ISeries<ApiPoint> BandLower1 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private PlotSeries _bandUpper2;

	private ISeries<ApiPoint> BandUpper2 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private PlotSeries _bandLower2;

	private ISeries<ApiPoint> BandLower2 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private PlotSeries _bandUpper3;

	private ISeries<ApiPoint> BandUpper3 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private PlotSeries _bandLower3;

	private ISeries<ApiPoint> BandLower3 => field ??= _bandUpper1.MapAndCache(GetApiPoint);

	[AllowNull]
	private BandInfo[] _bandInfos;

	[Parameter("Band ATR Period", GroupName = "Bands", Description = "Period of the band ATR")]
	public int BandAtrPeriod { get; set; } = 14;

	[Parameter("Show Opposite Bands", GroupName = "Bands", Description = "Whether opposite bands are shown")]
	public bool ShowOppositeBands { get; set; } = true;

	[Parameter("Show Bands 1", GroupName = "Bands", Description = "Whether bands 1 are shown")]
	public bool ShowBands1 { get; set; } = true;

	[Parameter("Show Bands 2", GroupName = "Bands", Description = "Whether bands 2 are shown")]
	public bool ShowBands2 { get; set; }

	[Parameter("Show Bands 3", GroupName = "Bands", Description = "Whether bands 3 are shown")]
	public bool ShowBands3 { get; set; }

	[Parameter("Band Multiplier 1", GroupName = "Bands", Description = "Multiplier for the bands 1")]
	public double BandMultiplier1 { get; set; } = 1;

	[Parameter("Band Multiplier 2", GroupName = "Bands", Description = "Multiplier for the bands 2")]
	public double BandMultiplier2 { get; set; } = 2;
	
	[Parameter("Band Multiplier 3", GroupName = "Bands", Description = "Multiplier for the bands 3")]
	public double BandMultiplier3 { get; set; } = 3;
	
	[Parameter("Band Color 1", GroupName = "Bands", Description = "Color of the band shading 1")]
	public Color BandColor1 { get; set; } = Color.Blue.With(opacity: 0.2f);

	[Parameter("Band Color 2", GroupName = "Bands", Description = "Color of the band shading 1")]
	public Color BandColor2 { get; set; } = Color.Orange.With(opacity: 0.2f);

	[Parameter("Band Color 3", GroupName = "Bands", Description = "Color of the band shading 1")]
	public Color BandColor3 { get; set; } = Color.Red.With(opacity: 0.2f);

	private ApiPoint GetApiPoint(int barIndex, double price)
	{
		var point = new Point(barIndex, price);

		return this.GetApiPoint(point);
	}

	private double GetBandAtr(int barIndex)
	{
		var totalAmount = 0.0d;
		
		var startBarIndex = Math.Max(0, barIndex - BandAtrPeriod + 1);
		var summandCount = barIndex - startBarIndex + 1;

		for (var index = startBarIndex; index <= barIndex; index++)
		{
			totalAmount += Bars.High[index] - Bars.Low[index];
		}

		var bandDeltaAtr = totalAmount / summandCount;

		if (IsTickRoundingEnabled)
		{
			bandDeltaAtr = Symbol.RoundToTick(bandDeltaAtr);
		}

		return bandDeltaAtr;
	}

	public void HideBandParameters(Parameters parameters)
	{
		if (!ShowBands1)
		{
			parameters.RemoveRange([nameof(BandMultiplier1), nameof(BandColor1)]);
		}

		if (!ShowBands2)
		{
			parameters.RemoveRange([nameof(BandMultiplier2), nameof(BandColor2)]);
		}

		if (!ShowBands3)
		{
			parameters.RemoveRange([nameof(BandMultiplier3), nameof(BandColor3)]);
		}
	}

	public void InitializeBands()
	{
		_bandUpper1 = new(BullishColor, LineStyle.Solid, 2);
		_bandLower1 = new(BearishColor, LineStyle.Solid, 2);
		
		_bandUpper2 = new(BullishColor, LineStyle.Solid, 2);
		_bandLower2 = new(BearishColor, LineStyle.Solid, 2);
		
		_bandUpper3 = new(BullishColor, LineStyle.Solid, 2);
		_bandLower3 = new(BearishColor, LineStyle.Solid, 2);

		var stopDotPoints = StopDots.MapAndCache(GetApiPoint);

		_bandInfos =
		[
			new BandInfo
			{
				Color = BandColor3,
				IsVisible = ShowBands3,
				IsAboveBaseline = true,
				UpperSeries = BandUpper3,
				LowerSeries = BandUpper2,
			},
			new BandInfo
			{
				Color = BandColor2,
				IsVisible = ShowBands2,
				IsAboveBaseline = true,
				UpperSeries = BandUpper2,
				LowerSeries = BandUpper1,
			},
			new BandInfo
			{
				Color = BandColor1,
				IsVisible = ShowBands1,
				IsAboveBaseline = true,
				UpperSeries = BandUpper1,
				LowerSeries = stopDotPoints,
			},
			new BandInfo
			{
				Color = BandColor1,
				IsVisible = ShowBands1,
				IsAboveBaseline = false,
				LowerSeries = BandLower1,
				UpperSeries = stopDotPoints,
			},
			new BandInfo
			{
				Color = BandColor2,
				IsVisible = ShowBands2,
				IsAboveBaseline = false,
				UpperSeries = BandLower1,
				LowerSeries = BandLower2,
			},
			new BandInfo
			{
				Color = BandColor3,
				IsVisible = ShowBands3,
				IsAboveBaseline = false,
				UpperSeries = BandLower2,
				LowerSeries = BandLower3,
			}
		];
	}

	public void CalculateBands(int barIndex)
	{
		var bandAtr = GetBandAtr(barIndex);

		CalculateBand(barIndex, bandAtr, BandMultiplier1, ref _bandLower1, ref _bandUpper1);
		CalculateBand(barIndex, bandAtr, BandMultiplier2, ref _bandLower2, ref _bandUpper2);
		CalculateBand(barIndex, bandAtr, BandMultiplier3, ref _bandLower3, ref _bandUpper3);
	}

    private void CalculateBand(int barIndex, double bandAtr, double bandMultiplier, ref readonly PlotSeries bandLower, ref readonly PlotSeries bandUpper)
    {
		bandLower[barIndex] = StopDots[barIndex] - bandMultiplier * bandAtr;
		bandUpper[barIndex] = StopDots[barIndex] + bandMultiplier * bandAtr;
	}

	public void RenderBands(IDrawingContext drawingContext)
	{
		if (!ShowBands1 && !ShowBands2 && !ShowBands3)
		{
			return;
		}

		var visibleBoundary = this.GetVisibleBoundary();
		var trendIntervals = _trendIntervals.GetVisibleDrawingParts(visibleBoundary);

		foreach (var trendInterval in trendIntervals)
		{
			var trend = trendInterval.Trend;

			var lastBarIndex = Bars.Count - 1;
			var endBarIndex = trendInterval.EndBarIndex;
			var startBarIndex = trendInterval.StartBarIndex;

			if (!endBarIndex.Equals(lastBarIndex))
			{
				endBarIndex--;
			}

			var intervalLength = endBarIndex - startBarIndex + 1;
			var intervalBarIndexRange = Enumerable.Range(startBarIndex, intervalLength);

			foreach (var bandInfo in _bandInfos)
			{
				var areDirectionsMatch = trend
					.Map(true, false)
					.Equals(bandInfo.IsAboveBaseline);					

				if (!bandInfo.IsVisible || !ShowOppositeBands && !areDirectionsMatch)
				{
					continue;
				}

				var series = bandInfo.Series;
				var seriesLineColor = bandInfo.IsAboveBaseline ? BullishColor : BearishColor;

				var upperPoints = intervalBarIndexRange.Select(barIndex => bandInfo.UpperSeries[barIndex]);
				var lowerPoints = intervalBarIndexRange.Select(barIndex => bandInfo.LowerSeries[barIndex]);

				var polygonPoints = lowerPoints
					.Reverse()
					.Concat(upperPoints);

				drawingContext.DrawPolygon(polygonPoints, bandInfo.Color);

				for (var barIndex = startBarIndex; barIndex < endBarIndex; barIndex++)
				{
					var startApiPoint = series[barIndex];
					var endApiPoint = series[barIndex + 1];

					drawingContext.DrawLine(startApiPoint, endApiPoint, seriesLineColor);
				}
			}
		}
	}

	private sealed class BandInfo
	{
		public required Color Color { get; init; }

		public required bool IsVisible { get; init; }

		public required bool IsAboveBaseline { get; init; }

		public required ISeries<ApiPoint> UpperSeries { get; init; }

		public required ISeries<ApiPoint> LowerSeries { get; init; }

		public ISeries<ApiPoint> Series => IsAboveBaseline ? UpperSeries : LowerSeries;
	}
}
