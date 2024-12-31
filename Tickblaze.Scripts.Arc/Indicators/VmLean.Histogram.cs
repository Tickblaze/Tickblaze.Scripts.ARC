using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tickblaze.Scripts.Arc;

// Todo: support histogram bar size.
public partial class VmLean
{
	private Macd _macd1 = new();
	private Macd _macd2 = new();
	private Macd _macd3 = new();
	private Macd _macd4 = new();

	[Parameter("Histogram Up Color", GroupName = "Histogram Parameters")]
	public Color HistogramUpColor { get; set; } = DrawingColor.LimeGreen;

	[Parameter("Histogram Down Color", GroupName = "Histogram Parameters")]
	public Color HistogramDownColor { get; set; } = DrawingColor.Maroon;

	

	[Plot("Histogram")]
	public PlotSeries Histogram { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	private void InitializeHistogram()
	{
		_macd1 = new Macd
		{
			FastPeriod = 8,
			SlowPeriod = 20,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_macd2 = new Macd
		{
			FastPeriod = 10,
			SlowPeriod = 20,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_macd3 = new Macd
		{
			FastPeriod = 20,
			SlowPeriod = 60,
			SignalPeriod = 20,
			Source = Bars.Close,
		};

		_macd4 = new Macd
		{
			FastPeriod = 60,
			SlowPeriod = 240,
			SignalPeriod = 20,
			Source = Bars.Close,
		};
	}

	public void CalculateHistogram(int barIndex)
	{

	}
}
