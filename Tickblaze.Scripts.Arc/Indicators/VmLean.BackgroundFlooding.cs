using Tickblaze.Scripts.Arc.Domain;

namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	private DrawingPartDictionary<int, Interval> _swing;

	private DrawingPartDictionary<int, Interval>

	[Parameter("Flooding Type", GroupName = "Background Flooding Parameters", Description = "Type of chart panel background flooding")]
	public BackgroundFloodingType FloodingType { get; set; } = BackgroundFloodingType.None;

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

	public void CalculateBackgroundFlooding(int barIndex)
	{

	}

	public enum BackgroundFloodingType
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
