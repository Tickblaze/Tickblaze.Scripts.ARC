using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Common;

public partial class VmLeanCore
{
	public int BandPeriod { get; set; }

	public double BandMultiplier { get; set; }

	public int MacdFastPeriod { get; set; }

	public required int MacdSlowPeriod { get; set; }

	[AllowNull]
	public Macd Macd { get; private set; }

	[AllowNull]
	public BollingerBands BollingerBands { get; private set; }
	
	private void InitializeMacdBb()
	{
		Macd = new Macd
		{
			Source = Bars.Close,
			SignalPeriod = BandPeriod,
            FastPeriod = MacdFastPeriod,
			SlowPeriod = MacdSlowPeriod,
        };

		BollingerBands = new BollingerBands
		{
			Period = BandPeriod,
			Source = Macd.Signal,
			Multiplier = BandMultiplier,
			SmoothingType = MovingAverageType.Simple,
		};
	}

	private void CalculateMacdBb()
    {
		Macd.Calculate();

		BollingerBands.Calculate();
	}
}
