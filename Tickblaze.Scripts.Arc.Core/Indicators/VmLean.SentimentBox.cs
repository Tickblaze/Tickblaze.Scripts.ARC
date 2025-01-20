using System.Diagnostics;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

// Todo: support sentiment box rendering below the histogram.
public partial class VmLean
{
	[Parameter("Show Sentiment Box", GroupName = "Sentiment Box Visuals", Description = "Whether sentiment box is shown")]
	public bool ShowSentimentBox { get; set; }

	[Parameter("Sentiment Location", GroupName = "Sentiment Box Visuals", Description = "Location of the sentiment box")]
	public SentimentLocation SentimentLocationValue { get; set; } = SentimentLocation.Right;

	[Parameter("Sentiment Text Font", GroupName = "Sentiment Box Visuals", Description = "Font of the sentiment box text")]
	public Font SentimentTextFont { get; set; } = new("Arial", 12);

	[Parameter("Neutral Bias Text", GroupName = "Sentiment Box Visuals", Description = "Text describing a neutral bias")]
	public string SentimentNeutralTrendBiasText { get; set; } = "Oscillation";

	[Parameter("Up-trend Bias Text", GroupName = "Sentiment Box Visuals", Description = "Text describing an up-trend bias")]
	public string SentimentUpTrendBiasText { get; set; } = "Up-trend";

	[Parameter("Down-trend Bias Text", GroupName = "Sentiment Box Visuals", Description = "Text describing a down-trend bias")]
	public string SentimentDownTrendBiasText { get; set; } = "Down-trend";

	[Parameter("Sentiment Background Color", GroupName = "Sentiment Box Visuals", Description = "Color of the sentiment box background")]
	public Color SentimentBackgroundColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Sentiment Text Color", GroupName = "Sentiment Box Visuals", Description = "Color of the sentiment box text")]
	public Color SentimentTextColor { get; set; } = Color.White;

	[Parameter("Sentiment Neutral Bias Color", GroupName = "Sentiment Box Visuals", Description = "Color of neutral bias text")]
	public Color SentimentNeutralColor { get; set; } = DrawingColor.MidnightBlue;

	[Parameter("Down-trend Bias Text Color", GroupName = "Sentiment Box Visuals", Description = "Color of the down-trend bias text")]
	public Color SentimentDownTrendBiasTextColor { get; set; } = Color.Black;

	[Parameter("Down-trend Bias Background Color", GroupName = "Sentiment Box Visuals", Description = "Color of the down-trend text background")]
	public Color SentimentDownTrendBiasBackgroundColor { get; set; } = Color.Red;

	[Parameter("Up-trend Bias Text Color", GroupName = "Sentiment Box Visuals", Description = "Color of the up-trend bias text")]
	public Color SentimentUpTrendBiasTextColor { get; set; } = Color.Black;

	[Parameter("Up-trend Bias Background Color", GroupName = "Sentiment Box Visuals", Description = "Color of the up-trend bias background")]
	public Color SentimentUpTrendBiasBackgroundColor { get; set; } = DrawingColor.Lime;

	public void GetCurrentTrendBiasValues(int lastVisibleIndex, out string trendBiasText,
		out Color trendBiasTextColor, out Color trendBiasBackgroundColor)
	{
		var trendBias = Swings.TrendBiases.GetAtOrDefault(lastVisibleIndex, Trend.None);

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

		const string structureBiasText = "STRUCTURE BIAS";

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
		var sentimentBoxTopLeft = new ApiPoint(sentimentBoxOffset, 0);

		var structureBiasTextOffset = (sentimentBoxWidth - structureBiasTextSize.Width) / 2.0;
		var structureBiasTopLeft = new ApiPoint
		{
			X = sentimentBoxOffset + structureBiasTextOffset,
			Y = (Chart.Height - trendBiasTextBoxHeight) / 2.0
		};

		var trendBiasRectangleWidth = 2 * HorizontalMargin + trendBiasTextMaxWidth;
		var trendBiasRectangleHeight = 2 * VerticalMargin + trendBiasTextMaxHeight;
		var trendBiasRectangleTopLeft = new ApiPoint
		{
			X = sentimentBoxOffset + HorizontalMargin,
			Y = structureBiasTopLeft.Y + structureBiasTextSize.Height + 2 * VerticalMargin,
		};

		var trendBiasTextOffset = (sentimentBoxWidth - trendBiasTextSize.Width) / 2.0;
		var trendBiasTextOffsetTopLeft = new ApiPoint
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
