namespace Tickblaze.Scripts.Arc;

public partial class SwingStructure
{
	public enum CalculationSource
	{
		[DisplayName("Current Bar")]
		CurrentBar,

		[DisplayName("Closed Bar")]
		ClosedBar,
	}
}
