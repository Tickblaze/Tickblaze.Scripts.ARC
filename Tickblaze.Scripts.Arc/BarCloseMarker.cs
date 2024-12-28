
namespace Tickblaze.Scripts.Arc;

public partial class BarCloseMarker : Indicator
{
	public BarCloseMarker()
	{
		
	}

	private double _currentPrice;
	private readonly DateTime _creationTimeUtc = DateTime.UtcNow;

	[Parameter("Bar Close WAV", GroupName = "Audio")]
	public string? BarCloseWav { get; set; }

	[Parameter("MarkerType", GroupName = "Marker Settings")]
	public MarkerType MarkerTypeValue { get; set; }

	[Parameter("Text Font", GroupName = "Marker Settings")]
	public Font TextFont { get; set; } = new Font("Arial", 12);

	[Parameter("Bar Close High Color", GroupName = "Marker Settings")]
	public Color BarCloseHighColor { get; set; } /*= Color.Lime;*/

	[Parameter("Current Price Color", GroupName = "Marker Settings")]
	public Color CurrentPriceColor { get; set; } = Color.Black;

	[Parameter("Bar Close Low Color", GroupName = "Marker Settings")]
	public Color BarCloseLowColor { get; set; } = Color.Red;

	[NumericRange(MinValue = 1)]
	[Parameter("Marker Width", GroupName = "Marker Settings")]
	public int MarkerWidth { get; set; } = 2;

	[NumericRange(MinValue = 1, MaxValue = 100)]
	[Parameter("Marker Opacity (%)", GroupName = "Marker Settings")]
	public int MarkerOpacityPercent { get; set; } = 50;

	[Parameter("Display Shadow", GroupName = "Shadow Settings")]
	public bool IsShadowDisplayed { get; set; } = true;

	[Parameter("Top Shadow Color", GroupName = "Shadow Settings")]
	public Color TopShadowColor { get; set; } /*= Color.Lime;*/

	[Parameter("Bottom Shadow Color", GroupName = "Shadow Settings")]
	public Color BottomShadowColor { get; set; } = Color.Red;

	public override void OnRender(IDrawingContext context)
	{
		if (Bars is not [.., var lastBar]
			|| lastBar is null
			|| lastBar.Time < _creationTimeUtc)
		{
			return;
		}
	}
}
