using Tickblaze.Scripts.Arc.Domain;
using Tickblaze.Scripts.Indicators;
using DomainPoint = Tickblaze.Scripts.Arc.Domain.Point;

namespace Tickblaze.Scripts.Arc;

// Todo: parameter descriptions.
// Todo: support for custom input series.
public partial class AtrTrailingStop : Indicator
{
	public AtrTrailingStop()
	{
		IsOverlay = true;
		ShortName = "TBC ATS";
		Name = "TB Core ATR Trailing Stop";
	}

	private int _currentIndex;
	private StrictTrend _currentTrend;
	private StrictTrend? _previousTrend;
	private AverageTrueRange _markerOffsetAtr = new();
	private AverageTrueRange _averageTrueRange = new();
	private readonly double _markerAtrMultiplier = 0.3d;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Period")]
	public int AtrPeriod { get; set; } = 10;

	[NumericRange(MinValue = 1)]
	[Parameter("ATR Multiplier")]
	public int AtrMultiplier { get; set; } = 3;

	public bool IsTickRoundingEnabled { get; set; }

	public bool ShowStopDots { get; set; } = true;

	public bool ShowStopLine { get; set; } = true;

	public int StopLineThickness { get; set; } = 1;

	public LineStyle StopLineStyle { get; set; } = LineStyle.Solid;

	public bool ShowReverseDots { get; set; }

	[Parameter("Show Triangles")]
	public bool ShowMarkers { get; set; }

	[Parameter("Triangle Size")]
	public int MarkerSize { get; set; } = 10;

	public Color BullishColor { get; set; } = Color.Blue;

	public Color BearishColor { get; set; } = Color.Red;

	[Plot("Stop Dot")]
	public PlotSeries StopDots { get; set; } = new(Color.Transparent, LineStyle.Dot, 2);

	[Plot("Reverse Dots")]
	public PlotSeries ReverseDots { get; set; } = new(Color.Transparent, LineStyle.Dot, 2);

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (!ShowMarkers)
		{
			parameters.Remove(nameof(MarkerSize));
		}

		if (!ShowStopLine)
		{
			parameters.RemoveRange([nameof(StopLineStyle), nameof(StopLineThickness)]);
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_currentIndex = default;
		_previousTrend = default;

		_markerOffsetAtr = new(256, MovingAverageType.Simple);
		_averageTrueRange = new(AtrPeriod, MovingAverageType.WellesWilder);
	}

	protected override void Calculate(int index)
	{
		var currentOpen = Bars.Open[index];
		var currentClose = Bars.Close[index];

		if (index is 0)
		{
			StopDots[index] = ReverseDots[index] = Bars.Close[index];

			_currentTrend = currentClose.EpsilonGreaterThanOrEquals(currentOpen) ? StrictTrend.Up : StrictTrend.Down;

			return;
		}

		if (_currentIndex.Equals(index))
		{
			return;
		}

		_currentIndex = index;
		_previousTrend = _currentTrend;

		var previousBarIndex = index - 1;
		var previousClose = Bars.Close[previousBarIndex];
		var previousTrailingStop = StopDots[previousBarIndex];
		var previousTrend = _previousTrend.GetValueOrDefault();
		
		var trendSignum = previousTrend.Map(1, -1);
		var trailingAmount = trendSignum * AtrMultiplier * _averageTrueRange[index - 1];
		
		ReverseDots[index] = previousClose + trailingAmount;

		if (previousTrend is StrictTrend.Up)
		{
			StopDots.Colors[index] = BullishColor;

			StopDots[index] = Math.Max(previousTrailingStop, previousClose - trailingAmount);
		}
		else
		{
			StopDots.Colors[index] = BearishColor;

			StopDots[index] = Math.Min(previousTrailingStop, previousClose - trailingAmount);
		}

		if (IsTickRoundingEnabled)
		{
			StopDots[index] = Symbol.RoundToTick(StopDots[index]);
			ReverseDots[index] = Symbol.RoundToTick(ReverseDots[index]);
		}

		var isTrendBreak = previousTrend is StrictTrend.Down && currentClose > previousTrailingStop
			|| previousTrend is StrictTrend.Up && currentClose < previousTrailingStop;

		if (isTrendBreak)
		{
			_currentTrend = previousTrend.GetOppositeTrend();

			// Todo: add triangles.
		}
	}
}
