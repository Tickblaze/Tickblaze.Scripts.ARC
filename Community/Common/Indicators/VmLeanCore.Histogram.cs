using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class VmLeanCore
{
	[AllowNull]
	private Macd _histomgraMacd1;

	[AllowNull]
	private Macd _histomgraMacd2;

	[AllowNull]
	private Macd _histomgraMacd3;

	[AllowNull]
	private Macd _histomgraMacd4;

	[AllowNull]
	public Series<double> Histogram { get; private set; }

	private void InitializeHistogram()
	{
		Histogram = [];

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

	private void CalculateHistogram(int barIndex)
	{
		Histogram[barIndex]
			= _histomgraMacd1.Histogram[barIndex]
			+ _histomgraMacd2.Histogram[barIndex]
			+ _histomgraMacd3.Histogram[barIndex]
			+ _histomgraMacd4.Histogram[barIndex];
	}
}
