using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

// Todo: parameter descriptions.
public partial class AtrTrailingStop : Indicator
{
	public AtrTrailingStop()
	{
		IsOverlay = true;
		ShortName = "TBC ATS";
		Name = "TB Core ATR Trailing Stop";
	}

	//private ComponentContainer<>
	private AverageTrueRange _averageTrueRange = new();

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period")]
	public int AtrPeriod { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Multiplier")]
	public int AtrMultiplier { get; set; } = 3;

	[Parameter("Show Triangles")]
	public bool ShowMarkers { get; set; }

	[Parameter("Triangle Size")]
	public int MarkerSize { get; set; } = 10;

	[Plot("Stop Dot")]
	public PlotSeries StopDots { get; set; } = new(Color.Transparent, LineStyle.Dot, 2);

	[Plot("Stop Line")]
	public PlotSeries TopStopLine { get; set; } = new(Color.Red);

	[Plot("Stop Line")]
	public PlotSeries BottomStopLine { get; set; } = new(Color.Blue);

    protected override Parameters GetParameters(Parameters parameters)
    {
		if (!ShowMarkers)
		{
			parameters.Remove(nameof(MarkerSize));
		}

		return parameters;
    }

    protected override void Initialize()
	{
		_averageTrueRange = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
	}

	protected override void Calculate(int index)
	{
		var x = AtrMultiplier * _averageTrueRange[index];
	}
}
