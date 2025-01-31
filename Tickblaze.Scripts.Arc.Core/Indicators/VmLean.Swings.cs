using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[AllowNull]
	private Swings Swings => _vmLeanCore.Swings;

	[NumericRange(MinValue = 1, MaxValue = 200)]
	[Parameter("Swing Strength", GroupName = "Swing Structure Parameters", Description = "Bar lookback to calculate swing high or low")]
	public int SwingStrength { get; set; } = 3;

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Deviation Multiplier", GroupName = "Swing Structure Parameters", Description = "ATR multipler of the minimum deviation required for a trend change")]
	public double SwingDeviationAtrMultiplier { get; set; }

	[NumericRange(MaxValue = double.MaxValue, Step = 0.1)]
	[Parameter("Swing Double Top/Bottom ATR Multiplier", GroupName = "Swing Structure Parameters", Description = "ATR multiplier of the maximum deviation ignored for a double tops or bottoms recognition")]
	public double SwingDtbAtrMultiplier { get; set; }

	private Swings InitializeSwings(bool forceReinitialization)
	{
		_vmLeanCore.SwingStrength = SwingStrength;
		_vmLeanCore.SwingDtbAtrMultiplier = SwingDtbAtrMultiplier;
		_vmLeanCore.SwingDeviationAtrMultiplier = SwingDeviationAtrMultiplier;

		_vmLeanCore.InitializeSwings(forceReinitialization);

		return Swings;
	}
}