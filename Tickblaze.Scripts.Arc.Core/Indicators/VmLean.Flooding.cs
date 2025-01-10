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
		var histogramTrends = Histogram.Map(histogram => histogram.ToTrend());

		_histogramFlooding = new Flooding
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor,
			DownTrendColor = FloodingBearishColor,
			TrendSeriesCollection = [histogramTrends],
		};

		_swingStructureFlooding = new Flooding
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor,
			DownTrendColor = FloodingBearishColor,
			TrendSeriesCollection = [_swings.BiasTrends],
		};

		_bothFlooding = new FloodingWithOverlaps
		{
			RenderTarget = this,
			UpTrendColor = FloodingBullishColor,
			DownTrendColor = FloodingBearishColor,
			OverlapUpTrendColor = FloodingDeepBullishColor,
			OverlapDownTrendColor = FloodingDeepBearishColor,
			TrendSeriesCollection = [_swings.BiasTrends, histogramTrends],
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
