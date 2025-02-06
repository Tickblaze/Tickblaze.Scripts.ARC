using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{	
	[AllowNull]
	private Flooding _histogramFlooding;

	[AllowNull]
	private Flooding _swingStructureFlooding;
	
	[AllowNull]
	private FloodingWithOverlaps _bothFlooding;

	[Parameter("Flooding Type", GroupName = "Flooding Visuals", Description = "Type of the flooding")]
	public FloodingType FloodingTypeValue { get; set; } = FloodingType.None;

	[NumericRange(MinValue = 0, MaxValue = 100)]
	[Parameter("Flooding Opacity", GroupName = "Flooding Visuals", Description = "Opacity of the flooding")]
	public int FloodingOpacity { get; set; } = 30;

	[Parameter("Flooding Deep Bullish Color", GroupName = "Flooding Visuals", Description = "Color of the deep bullish flooding")]
	public Color FloodingDeepBullishColor { get; set; } = DrawingColor.DarkGreen;

	[Parameter("Flooding Bullish Color", GroupName = "Flooding Visuals", Description = "Color of the bullish flooding")]
	public Color FloodingBullishColor { get; set; } = Color.Green;

	[Parameter("Flooding Bearish Color", GroupName = "Flooding Visuals", Description = "Color of the deep bearish flooding")]
	public Color FloodingBearishColor { get; set; } = Color.Red;

	[Parameter("Flooding Deep Bearish Color", GroupName = "Flooding Visuals", Description = "Color of the bearish flooding")]
	public Color FloodingDeepBearishColor { get; set; } = DrawingColor.DarkRed;

	private Flooding? GetFlooding()
	{
		return FloodingTypeValue switch
		{
			FloodingType.None => default,
			FloodingType.Histogram => _histogramFlooding,
			FloodingType.Structure => _swingStructureFlooding,
			FloodingType.Both => _bothFlooding,
			_ => throw new UnreachableException(),
		};
	}

	public void InitializeFlooding(bool forceReinitialization)
	{
		var floodingOpacity = FloodingOpacity / 100.0f;

		var histogramTrends = Histogram.Select(histogramValue => histogramValue.ToTrend());

		_histogramFlooding = new Flooding
		{
			Bars = Bars,
			IsMtf = false,
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [histogramTrends],
		};

		_swingStructureFlooding = new Flooding
		{
			Bars = Bars,
			IsMtf = false,
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [Swings.TrendBiases],
		};

		_bothFlooding = new FloodingWithOverlaps
		{
			Bars = Bars,
			IsMtf = false,
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			OverlapUpTrendColor = FloodingDeepBullishColor.With(opacity: floodingOpacity),
			OverlapDownTrendColor = FloodingDeepBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [Swings.TrendBiases, histogramTrends],
		};

		if (forceReinitialization)
		{
			_histogramFlooding.Reinitialize(this);

			_swingStructureFlooding.Reinitialize(this);

			_bothFlooding.Reinitialize(this);
		}
	}

	public void CalculateFlooding(int barIndex)
	{
		_bothFlooding.Calculate();
		
		_histogramFlooding.Calculate();
		
		_swingStructureFlooding.Calculate();

		var flooding = GetFlooding();

		BackgroundColor[barIndex] = flooding?.BackgroundColor[barIndex];
	}

	private void UpdateFloodingType(FloodingType flodingType)
	{
		FloodingTypeValue = flodingType;

		var flooding = GetFlooding();
		
		for (var barIndex = 0; barIndex < Bars.Count; barIndex++)
		{
			BackgroundColor[barIndex] = flooding?.BackgroundColor[barIndex];
		}
	}

    public enum FloodingType
	{
		[DisplayName("None")]
		None,

		[DisplayName("Histogram")]
		Histogram,

		[DisplayName("Structure")]
		Structure,

		[DisplayName("Both")]
		Both
	}
}
