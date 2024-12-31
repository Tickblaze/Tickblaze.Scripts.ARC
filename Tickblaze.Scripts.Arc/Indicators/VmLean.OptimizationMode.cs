namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
    public enum OptimizationMode
	{
		[DisplayName("None")]
		None,

		[DisplayName("Min")]
		Minimum,

		[DisplayName("Max")]
		Maximum,
	}
}
