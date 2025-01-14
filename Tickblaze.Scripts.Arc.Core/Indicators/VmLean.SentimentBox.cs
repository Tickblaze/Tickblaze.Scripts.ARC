using System.Diagnostics;
using Tickblaze.Scripts.Arc.Common;
using Point = Tickblaze.Scripts.Api.Models.Point;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	[Parameter("Show Sentiment Box", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is shown")]
	public bool ShowSentimentBox { get; set; }

	//[Parameter("Sentiment Overprint", GroupName = "Sentiment Box Parameters", Description = "Whether sentiment box is printed on top of the histogram")]
	//public bool IsSentimentOverprinted { get; set; }

	[Parameter("Sentiment Location", GroupName = "Sentiment Box Parameters", Description = "Location of sentiment box")]
	public SentimentLocation SentimentLocationValue { get; set; } = SentimentLocation.Right;

	[Parameter("Sentiment Background Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentBackgroundColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Sentiment Text Font", GroupName = "Sentiment Box Parameters", Description = "Font for text in the sentiment box")]
	public Font SentimentTextFont { get; set; } = new("Arial", 12);

	[Parameter("Sentiment Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentTextColor { get; set; } = Color.White;

	[Parameter("Down Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for down trend bias")]
	public string SentimentDownTrendBiasText { get; set; } = "Down Trend";

	[Parameter("Down Trend Bias Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentDownTrendBiasTextColor { get; set; } = Color.Black;

	[Parameter("Down Trend Bias Background Color", GroupName = "Sentiment Box Parameters", Description = "Color for bearish acceleration")]
	public Color SentimentDownTrendBiasBackgroundColor { get; set; } = Color.Red;

	[Parameter("Up Trend Bias Text Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentUpTrendBiasTextColor { get; set; } = Color.Black;

	[Parameter("Up Trend Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for up trend bias")]
	public string SentimentUpTrendBiasText { get; set; } = "Up Trend";

	[Parameter("Up Trend Bias Background Color", GroupName = "Sentiment Box Parameters", Description = "Color for bullish acceleration")]
	public Color SentimentUpTrendBiasBackgroundColor { get; set; } = DrawingColor.Lime;

	[Parameter("Neutral Bias Text", GroupName = "Sentiment Box Parameters", Description = "Text for neutral bias")]
	public string SentimentNeutralTrendBiasText { get; set; } = "Oscillation";
	
	[Parameter("Sentiment Neutral Bias Color", GroupName = "Sentiment Box Parameters")]
	public Color SentimentNeutralColor { get; set; } = DrawingColor.MidnightBlue;

	public void GetCurrentTrendBiasValues(int lastVisibleIndex, out string trendBiasText,
		out Color trendBiasTextColor, out Color trendBiasBackgroundColor)
	{
		var trendBias = _swings.TrendBiases.GetAtOrDefault(lastVisibleIndex, Trend.None);

		(trendBiasText, trendBiasTextColor, trendBiasBackgroundColor) = trendBias switch
		{
			Trend.None => (SentimentNeutralTrendBiasText, SentimentTextColor, SentimentNeutralColor),
			Trend.Up => (SentimentUpTrendBiasText, SentimentUpTrendBiasTextColor, SentimentUpTrendBiasBackgroundColor),
			Trend.Down => (SentimentDownTrendBiasText, SentimentDownTrendBiasTextColor, SentimentDownTrendBiasBackgroundColor),
			_ => throw new UnreachableException(),
		};
	}

	public void RenderSentimentBox(IDrawingContext drawingContext)
	{
		if (!ShowSentimentBox)
		{
			return;
		}

		const string structureBiasText = "STRCTURE BIAS";

		IEnumerable<string> trendBiasTexts =
		[
			structureBiasText,
			SentimentUpTrendBiasText,
			SentimentDownTrendBiasText,
			SentimentNeutralTrendBiasText,
		];

		GetCurrentTrendBiasValues(Chart.LastVisibleBarIndex, out var trendBiasText, out var trendBiasTextColor, out var trendBiasBackgroundColor);

		var structureBiasTextSize = drawingContext.MeasureText(structureBiasText, SentimentTextFont);
		
		var trendBiasTextSizes = trendBiasTexts.Select(trendBiasLabel => drawingContext.MeasureText(trendBiasLabel, SentimentTextFont));
		var trendBiasTextMaxWidth = trendBiasTextSizes.Max(size => size.Width);
		var trendBiasTextMaxHeight = trendBiasTextSizes.Max(size => size.Height);
		var trendBiasTextSize = drawingContext.MeasureText(trendBiasText, SentimentTextFont);
		var trendBiasTextBoxHeight = 4 * VerticalMargin + structureBiasTextSize.Height + trendBiasTextMaxHeight;
		
		var sentimentBoxWidth = 4 * HorizontalMargin + trendBiasTextMaxWidth;
		var sentimentBoxOffset = SentimentLocationValue switch
		{
			SentimentLocation.Left => 0,
			SentimentLocation.Right => Chart.Width - sentimentBoxWidth,
			_ => throw new UnreachableException()
		};
		var sentimentBoxTopLeft = new Point(sentimentBoxOffset, 0);

		var structureBiasTextOffset = (sentimentBoxWidth - structureBiasTextSize.Width) / 2.0;
		var structureBiasTopLeft = new Point
		{
			X = sentimentBoxOffset + structureBiasTextOffset,
			Y = (Chart.Height - trendBiasTextBoxHeight) / 2.0
		};

		var trendBiasRectangleWidth = 2 * HorizontalMargin + trendBiasTextMaxWidth;
		var trendBiasRectangleHeight = 2 * VerticalMargin + trendBiasTextMaxHeight;
		var trendBiasRectangleTopLeft = new Point
		{
			X = sentimentBoxOffset + HorizontalMargin,
			Y = structureBiasTopLeft.Y + structureBiasTextSize.Height + 2 * VerticalMargin,
		};

		var trendBiasTextOffset = (sentimentBoxWidth - trendBiasTextSize.Width) / 2.0;
		var trendBiasTextOffsetTopLeft = new Point
		{
			X = sentimentBoxOffset + trendBiasTextOffset,
			Y = trendBiasRectangleTopLeft.Y + VerticalMargin,
		};

		drawingContext.DrawRectangle(sentimentBoxTopLeft, sentimentBoxWidth, Chart.Height, SentimentBackgroundColor);

		drawingContext.DrawText(structureBiasTopLeft, structureBiasText, SentimentTextColor, SentimentTextFont);

		drawingContext.DrawRectangle(trendBiasRectangleTopLeft, trendBiasRectangleWidth, trendBiasRectangleHeight, trendBiasBackgroundColor, trendBiasTextColor);

		drawingContext.DrawText(trendBiasTextOffsetTopLeft, trendBiasText, trendBiasTextColor, SentimentTextFont);
	}

	public enum SentimentLocation
	{
		[DisplayName("Left")]
		Left,

		[DisplayName("Right")]
		Right,
	}
}
