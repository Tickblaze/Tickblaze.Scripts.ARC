namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	[Parameter("Show Sentiment Box", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is shown")]
	public bool ShowSentimentBox { get; set; }

	[Parameter("Sentiment Overprint", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is printed on top of the histogram")]
	public bool IsSentimentOverprinted { get; set; }

	[Parameter("Sentiment Location", GroupName = "Sentiment Box Parameters", Description = "Location of sentiment box")]
	public SentimentLocation SentimentLocationValue { get; set; } = SentimentLocation.Right;

	[Parameter("Sentiment Background Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentBackgroundColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Sentiment Text Font", GroupName = "Sentiment Box Parameters", Description = "Font for text in the sentiment box")]
	public Font SentimentTextFont { get; set; } = new("Arial", 12);

	[Parameter("Sentiment Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentTextColor { get; set; } = Color.White;

	[Parameter("Bearish Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentBearishTextColor { get; set; } = Color.Black;

	[Parameter("Bullish Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentBullishTextColor { get; set; } = Color.Black;

	[Parameter("Neutral Condition Text", GroupName = "Sentiment Box Parameters", Description = "Text for condition without bullish or bearish indications")]
	public string SentimentNeutralConditionText { get; set; } = "Neutral";

	[Parameter("Sentiment Neutral Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentNeutralColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Up Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for up trend bias")]
	public string SentimentUpTrendBiasText { get; set; } = "Up Trend";

	[Parameter("Neutral Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for neutral bias")]
	public string SentimentNeutralTrendBiasText { get; set; } = "Oscillation";

	[Parameter("Down Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for down trend bias")]
	public string SentimentDownTrendBiasText { get; set; } = "Down Trend";

	public enum SentimentLocation
	{
		[DisplayName("Left")]
		Left,

		[DisplayName("Right")]
		Right,
	}
}
