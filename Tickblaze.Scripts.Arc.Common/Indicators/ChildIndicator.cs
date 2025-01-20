namespace Tickblaze.Scripts.Arc.Common;

public abstract class ChildIndicator : Indicator
{
	public IChartObject? RenderTarget { get; init; }

	public new IChart? Chart => RenderTarget?.Chart;

	public new IChartScale? ChartScale => RenderTarget?.ChartScale;

	public void Reinitialize()
	{
		Initialize();

		Calculate();
	}
}
