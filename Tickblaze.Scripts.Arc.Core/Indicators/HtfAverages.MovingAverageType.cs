namespace Tickblaze.Scripts.Arc.Core;

public partial class HtfAverages
{
	public enum MovingAverageType
	{
		[DisplayName("SMA")]
		Simple,

		[DisplayName("EMA")]
		Exponential,
	}
}
