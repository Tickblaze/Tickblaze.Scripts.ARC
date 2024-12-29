namespace Tickblaze.Scripts.Arc;

public partial class LeadersAndLaggers
{
	public enum ResetType
	{
		[DisplayName("Session")]
		Session,

		[DisplayName("Chart Start")]
		ChartStart,

		[DisplayName("Date")]
		Custom,
	}
}
