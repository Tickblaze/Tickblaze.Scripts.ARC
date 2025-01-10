using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class AtrTrailingStop
{
	public int AtrBandPeriod { get; set; } = 14;

	public bool ShowBand1 { get; set; } = true;

	public int Band1Multiplier { get; set; } = 1;

	public Color Band1Color { get; set; } = Color.Blue.With(opacity: 0.2f);

	public bool ShowBand2 { get; set; }

	public int Band2Multiplier { get; set; } = 2;

	public Color Band2Color { get; set; } = Color.Orange.With(opacity: 0.2f);

	public bool ShowBand3 { get; set; }

	public int Band3Multiplier { get; set; } = 3;

	public Color Band3Color { get; set; } = Color.Red.With(opacity: 0.2f);

	public void CalculateBands(int barIndex)
	{
		var bandAtr = GetBandAtr(barIndex);

	}

    private double GetBandAtr(int barIndex)
    {
		var startBarIndex = Math.Max(0, barIndex - AtrBandPeriod);
		var endBarIndex = barIndex;
		var totalAmount = 0.0d;

		for (var index = startBarIndex; index < endBarIndex; index++)
		{
			totalAmount += Bars.High[index] - Bars.Low[index];
		}

		var bandAtr = totalAmount / AtrBandPeriod;

		if (IsTickRoundingEnabled)
		{
			bandAtr = Symbol.RoundToTick(bandAtr);
		}

		return bandAtr;
    }
}
