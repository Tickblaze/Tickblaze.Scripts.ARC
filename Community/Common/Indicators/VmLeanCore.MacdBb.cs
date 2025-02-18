using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Community;

public partial class VmLeanCore
{
	[AllowNull]
	private StandardDeviation _standardDeviation;

	public int BandPeriod { get; set; }

	public double BandMultiplier { get; set; }

	public int MacdFastPeriod { get; set; }

	public required int MacdSlowPeriod { get; set; }

	[AllowNull]
	public Macd Macd { get; private set; }

	[AllowNull]
	public Series<double> LowerBand { get; private set; }

	[AllowNull]
	public Series<double> UpperBand { get; private set; }

	private void InitializeMacdBb()
	{
		LowerBand = [];
		UpperBand = [];

		Macd = new Macd
		{
			Source = Bars.Close,
			SignalPeriod = BandPeriod,
            FastPeriod = MacdFastPeriod,
			SlowPeriod = MacdSlowPeriod,
        };

		_standardDeviation = new StandardDeviation
		{
			Period = BandPeriod,
			Source = Macd.Result,
			SmoothingType = MovingAverageType.Simple,
		};
	}

	private void CalculateMacdBb(int barIndex)
    {
		var macdAverage = Macd.Signal[barIndex];
		var standardDeviation = _standardDeviation[barIndex];

		LowerBand[barIndex] = macdAverage - BandMultiplier * standardDeviation;
		UpperBand[barIndex] = macdAverage + BandMultiplier * standardDeviation;
	}
}
