namespace Tickblaze.Community;

public abstract class ChildIndicator : Indicator
{
	public IChartObject? RenderTarget { get; init; }

	public new IChart? Chart => RenderTarget?.Chart;

	public new IChartScale? ChartScale => RenderTarget?.ChartScale;
}
