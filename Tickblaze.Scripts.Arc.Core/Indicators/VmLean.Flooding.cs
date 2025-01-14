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

	[Parameter("Flooding Type", GroupName = "Background Flooding Parameters", Description = "Type of chart panel background flooding")]
	public FloodingType FloodingTypeValue { get; set; } = FloodingType.None;

	[NumericRange(MinValue = 0, MaxValue = 100)]
	[Parameter("Flooding Opacity", GroupName = "Background Flooding Parameters", Description = "Opacity of chart panel background flooding")]
	public int FloodingOpacity { get; set; } = 30;

	[Parameter("Flooding Deep Bullish Color", GroupName = "Background Flooding Parameters")]
	public Color FloodingDeepBullishColor { get; set; } = DrawingColor.DarkGreen;

	[Parameter("Flooding Bullish Color", GroupName = "Background Flooding Parameters")]
	public Color FloodingBullishColor { get; set; } = Color.Green;

	[Parameter("Flooding Opposite Color", GroupName = "Background Flooding Parameters")]
	public Color FloodingOppositeColor { get; set; } = Color.Gray;

	[Parameter("Flooding Bearish Color", GroupName = "Background Flooding Parameters")]
	public Color FloodingBearishColor { get; set; } = Color.Red;

	[Parameter("Flooding Deep Bearish Color", GroupName = "Background Flooding Parameters")]
	public Color FloodingDeepBearishColor { get; set; } = DrawingColor.DarkRed;

	public void InitializeFlooding()
	{
		var floodingOpacity = FloodingOpacity / 100.0f;

		var histogramTrends = Histogram.Map(histogramValue => histogramValue.ToTrend());

		_histogramFlooding = new Flooding
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [histogramTrends],
		};

		_swingStructureFlooding = new Flooding
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [_swings.TrendBiases],
		};

		_bothFlooding = new FloodingWithOverlaps
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor.With(opacity: floodingOpacity),
			DownTrendColor = FloodingBearishColor.With(opacity: floodingOpacity),
			OverlapUpTrendColor = FloodingDeepBullishColor.With(opacity: floodingOpacity),
			OverlapDownTrendColor = FloodingDeepBearishColor.With(opacity: floodingOpacity),
			TrendSeriesCollection = [_swings.TrendBiases, histogramTrends],
		};
	}

	public void CalculateFlooding(int barIndex)
	{
		_bothFlooding.Calculate();
		
		_histogramFlooding.Calculate();
		
		_swingStructureFlooding.Calculate();
	}

	public void RenderFlooding(IDrawingContext drawingContext)
	{
		var flooding = FloodingTypeValue switch
		{
			FloodingType.None => default,
			FloodingType.Histogram => _histogramFlooding,
			FloodingType.Structure => _swingStructureFlooding,
			FloodingType.Both => _bothFlooding,
			_ => throw new UnreachableException(),
		};

		flooding?.OnRender(drawingContext);
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
