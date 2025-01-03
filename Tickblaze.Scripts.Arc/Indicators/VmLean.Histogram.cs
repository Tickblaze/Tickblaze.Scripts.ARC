namespace Tickblaze.Scripts.Arc;

// Todo: support histogram bar size.
public partial class VmLean
{
	private Macd _histomgraMacd1 = new();
	private Macd _histomgraMacd2 = new();
	private Macd _histomgraMacd3 = new();
	private Macd _histomgraMacd4 = new();

	[Parameter("Histogram Up Color", GroupName = "Histogram Parameters")]
	public Color HistogramUpColor { get; set; } = DrawingColor.LimeGreen;

	[Parameter("Histogram Down Color", GroupName = "Histogram Parameters")]
	public Color HistogramDownColor { get; set; } = DrawingColor.Maroon;

	[Plot("Histogram")]
	public PlotSeries Histogram { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	private void InitializeHistogram()
	{
		_histomgraMacd1 = new Macd
		{
			FastPeriod = 8,
			SlowPeriod = 20,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_histomgraMacd2 = new Macd
		{
			FastPeriod = 10,
			SlowPeriod = 20,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_histomgraMacd3 = new Macd
		{
			FastPeriod = 20,
			SlowPeriod = 60,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_histomgraMacd4 = new Macd
		{
			FastPeriod = 60,
			SlowPeriod = 240,
			SignalPeriod = 20,
			Source = Bars.Close,
		};
	}

	public void CalculateHistogram(int barIndex)
	{
		if (barIndex is 0)
		{
			return;
		}

		var currentValue = Histogram[barIndex]
			= _histomgraMacd1.Histogram[barIndex]
			+ _histomgraMacd2.Histogram[barIndex]
			+ _histomgraMacd3.Histogram[barIndex]
			+ _histomgraMacd4.Histogram[barIndex];

		if (currentValue > 0)
		{
			Histogram.Colors[barIndex] = HistogramUpColor;
		}
		else if (currentValue < 0)
		{
			Histogram.Colors[barIndex] = HistogramUpColor;
		}
		else
		{
			Histogram.Colors[barIndex] = Histogram.Colors[barIndex - 1];
		}
	}
}
