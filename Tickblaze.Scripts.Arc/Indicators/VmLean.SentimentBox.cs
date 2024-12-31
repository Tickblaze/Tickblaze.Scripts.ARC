using System.ComponentModel.DataAnnotations;

namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	[Parameter("Show Sentiment Box", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is shown")]
	public bool ShowSentimentBox { get; set; }

	[Parameter("Sentiment Overprint", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is printed on top of the histogram")]
	public bool IsSentimentOverprinted { get; set; }

	[Parameter("Sentiment Location", GroupName = "Sentiment Box Parameters", Description = "Location of sentiment box")]
	public SentimentLocation SentimentLocationValue { get; set; } = SentimentLocation.RightAligned;

	[Parameter("Sentiment Background Color", GroupName = "Sentiment Box Parameters", Description = "Color for background in the sentiment box")]
	public Color SentimentBackgroundColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Sentiment Text Font", GroupName = "Sentiment Box Parameters", Description = "Font for text in the sentiment box")]
	public Font SentimentTextFont { get; set; } = new("Arial", 12);

	[Parameter("Sentiment Text Color", GroupName = "Sentiment Box Parameters", Description = "Color for text in the sentiment box")]
	public Color SentimentTextColor { get; set; } = Color.White;

	[Parameter("Bearish Text Color", GroupName = "Sentiment Box Parameters", Description = "Color for bearish text")]
	public Color SentimentBearishTextColor { get; set; } = Color.Black;

	[Parameter("Bullish Text Color", GroupName = "Sentiment Box Parameters", Description = "Color for bullish text")]
	public Color SentimentBullishTextColor { get; set; } = Color.Black;

	[Parameter("Bearish Acceleration Text", GroupName = "Sentiment Box Parameters", Description = "Text for bearish acceleration")]
	public string SentimentBearishAccText { get; set; } = "Bearish Acc";

	[Parameter("Bearish Acceleration Color", GroupName = "Sentiment Box Parameters", Description = "Color for bearish acceleration")]
	public Color SentimentBearishAccColor { get; set; } = Color.Red;

	[Parameter("Bullish Acceleration Text", GroupName = "Sentiment Box Parameters", Description = "Text for bullish acceleration")]
	public string SentimentBullishAccText { get; set; } = "Bullish Acc";

	[Parameter("Bearish Acceleration Color", GroupName = "Sentiment Box Parameters", Description = "Color for bullish acceleration")]
	public Color SentimentBullishAccColor { get; set; } = DrawingColor.Lime;

	[Parameter("Confirmed Bearish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for confirmed bearish divergence")]
	public string SentimentBearishCDivText { get; set; } = "Bearish Div (C)";

	[Parameter("Confirmed Hidden Bearish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for confirmed hidden bearish divergence")]
	public string SentimentBearishChDivText { get; set; } = "Bearish HDiv (C)";

	[Parameter("Potential Bearish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for potential bearish divergence")]
	public string SentimentBearishPDivText { get; set; } = "Bearish Div (P)";

	[Parameter("Potential Hidden Bearish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for potential hidden bearish divergence")]
	public string SentimentBearishPhDivText { get; set; } = "Bearish HDiv (P)";

	[Parameter("Bearish Divergence Color", GroupName = "Sentiment Box Parameters", Description = "Color for bearish acceleration")]
	public Color SentimentBearishDivColor { get; set; } = Color.Red;

	[Parameter("Confirmed Bullish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for confirmed bullish divergence")]
	public string SentimentBullishCDivText { get; set; } = "Bearish Div (C)";

	[Parameter("Confirmed Hidden Bullish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for confirmed hidden bullish divergence")]
	public string SentimentBullishChDivText { get; set; } = "Bearish HDiv (C)";

	[Parameter("Potential Bullish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for potential bullish divergence")]
	public string SentimentBullishPDivText { get; set; } = "Bearish Div (P)";

	[Parameter("Potential Hidden Bullish Divergence Text", GroupName = "Sentiment Box Parameters", Description = "Text for potential hidden bullish divergence")]
	public string SentimentBullishPhDivText { get; set; } = "Bearish HDiv (P)";

	[Parameter("Bullish Divergence Color", GroupName = "Sentiment Box Parameters", Description = "Color for bullish acceleration")]
	public Color SentimentBullishDivColor { get; set; } = DrawingColor.Lime;

	[Parameter("Neutral Condition Text", GroupName = "Sentiment Box Parameters", Description = "Text for condition without bullish or bearish indications")]
	public string SentimentNeutralConditionText { get; set; } = "Neutral";

	[Parameter("Sentiment Outline Color", GroupName = "Sentiment Box Parameters", Description = "Color for condition without bullish or bearish indications")]
	public Color SentimentOutlineColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Up Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for up trend bias")]
	public string SentimentUpTrendBiasText { get; set; } = "Up Trend";

	[Parameter("Neutral Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for neutral bias")]
	public string SentimentNeutralTrendBiasText { get; set; } = "Oscillation";

	[Parameter("Down Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for down trend bias")]
	public string SentimentDownTrendBiasText { get; set; } = "Down Trend";

	public enum SentimentLocation
	{
		[DisplayName("None")]
		None,

		[DisplayName("Left")]
		LeftAligned,

		[DisplayName("Right")]
		RightAligned,
	}
}
