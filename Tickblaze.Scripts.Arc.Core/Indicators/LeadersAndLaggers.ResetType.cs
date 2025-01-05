namespace Tickblaze.Scripts.Arc.Core;

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
