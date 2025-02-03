using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: support histogram bar size.
public partial class VmLean
{
	[Parameter("Histogram Up Color", GroupName = "Histogram Visuals", Description = "Color of the positive histogram values")]
	public Color HistogramUpColor { get; set; } = DrawingColor.LimeGreen;

	[Parameter("Histogram Down Color", GroupName = "Histogram Visuals", Description = "Color of the negative histogram values")]
	public Color HistogramDownColor { get; set; } = DrawingColor.Maroon;

	[Plot("Histogram")]
	public PlotSeries Histogram { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	private void CalculateHistogram(int barIndex)
	{
		var currentValue = Histogram[barIndex] = _vmLeanCore.Histogram[barIndex];

		Histogram.Colors[barIndex] = currentValue.CompareTo(0) switch
		{
			> 0 => HistogramUpColor,
			< 0 => HistogramDownColor,
			0 => Histogram.Colors.GetAtOrDefault(barIndex - 1, Color.Transparent),
		};
	}
}
