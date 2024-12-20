
using Tickblaze.Scripts.Api.Enums;

namespace Tickblaze.Scripts.Arc;

/// <summary>
/// ARC ATRTrailingStop [AATS]
/// </summary>
public partial class AtrTrailingStop : Indicator
{
    public AtrTrailingStop()
    {
        IsOverlay = true;
        ShortName = "AATS";
        Name = "ARC ATR Trailing Stop";
    }

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Period", GroupName = "Input Parameters")]
    public int AtrPeriod { get; set; } = 14;

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Multiplier", GroupName = "Input Parameters")]
    public int AtrMultiplier { get; set; } = 3;

    [Parameter("Show stop dots", GroupName = "Display Options")]
    public bool ShowStopDots { get; set; } = true;

    [Parameter("Show stop line", GroupName = "Display Options")]
    public bool ShowStopLine { get; set; } = true;

    [Parameter("Show Triangles", GroupName = "Display Options")]
    public bool ShowMarkers { get; set; }

    [Parameter("Bullish Color", GroupName = "Plots")]
    public Color BullishColor { get; set; } = Color.Blue;

    [Parameter("Bearish Color", GroupName = "Plots")]
    public Color BearishColor { get; set; } = Color.Red;

    [Parameter("Round band to tick")]
    public bool RoundBandToTick { get; set; } = true;

    [Parameter("Show opposite bands")]
    public bool ShowOppositeBands { get; set; } = true;

    [Plot("Stop Dot")]
    public PlotSeries StopDot { get; set; } = new("#808080", PlotStyle.Dot);

    [Plot("Stop Line")]
    public PlotSeries StopLine { get; set; } = new("#808080", PlotStyle.Hash);

    [Plot("Reverse Dot")]
    public PlotSeries ReverseDot { get; set; } = new("#000000", PlotStyle.Dot);

    private AverageTrueRange _offsetSeries;
    private DataSeries _preliminaryTrend, _trend, _currentStopLong, _currentStopShort;
    private int _regionID;
    private int _priorIndex = -1;

    protected override void Initialize()
    {
        _preliminaryTrend = new DataSeries();
        _trend = new DataSeries();
        _currentStopLong = new DataSeries();
        _currentStopShort = new DataSeries();
        _offsetSeries = new AverageTrueRange(AtrPeriod, MovingAverageType.Simple);
    }

    protected override void Calculate(int index)
    {
        var isFirstTickOfBar = _priorIndex != index;
        var close0 = Bars[index].Close;

        StopDot[index] = close0;
        StopLine[index] = close0;
        ReverseDot[index] = close0;

        if (index < 2)
        {
            _preliminaryTrend[index] = 1.0;
            _trend[index] = 1.0;
            _currentStopLong[index] = close0;
            _currentStopShort[index] = close0;
            return;
        }

        var bandATR = 0.0;

        if (index > ATRBandPeriod)
        {
            var sum = 0.0;
            for (var i = 1; i <= ATRBandPeriod; i++)
            {
                sum += Bars[index - i].High - Bars[index - i].Low;
            }

            bandATR = sum / ATRBandPeriod;
            if (RoundBandToTick)
            {
                bandATR = Bars.Symbol.RoundToTick(bandATR);
            }
        }

        var trailingAmount = AtrMultiplier * Math.Max(Bars.Symbol.TickSize, _offsetSeries[index - 1]);
        var close1 = Bars[^1].Close;

        if (_preliminaryTrend[^1] > 0.5)
        {
            _currentStopLong[index] = Math.Max(_currentStopLong[^1], Math.Min(close1 - trailingAmount, close1 - Bars.Symbol.TickSize));
            _currentStopShort[index] = close1 + trailingAmount;
            StopDot[index] = _currentStopLong[index];
            StopLine[index] = _currentStopLong[index];
            ReverseDot[index] = _currentStopShort[index];
        }
        else
        {
            _currentStopShort[index] = Math.Min(_currentStopShort[^1], Math.Max(close1 + trailingAmount, close1 + Bars.Symbol.TickSize));
            _currentStopLong[index] = close1 - trailingAmount;
            StopDot[index] = _currentStopShort[index];
            StopLine[index] = _currentStopShort[index];
            ReverseDot[index] = _currentStopLong[index];
        }

        if (_preliminaryTrend[^1] > 0.5 && close0 < _currentStopLong[index])
        {
            _preliminaryTrend[index] = -1.0;
        }
        else if (_preliminaryTrend[^1] < -0.5 && close0 > _currentStopShort[index])
        {
            _preliminaryTrend[index] = 1.0;
        }
        else
        {
            _preliminaryTrend[index] = _preliminaryTrend[^1];
        }

        if (isFirstTickOfBar)
        {
            _trend[index] = _preliminaryTrend[^1];
        }

        if (_trend[index - 2] != _trend[^1])
        {
            _regionID = index;
        }

        if (_regionID > 0)
        {
            foreach (var band in _bandPlots)
            {
                var bandpts = bandATR * _bandMults[band.Key];
                if (ShowOppositeBands)
                {
                    _bandPlots[band.Key][index] = StopLine[index] + bandpts;
                }
                else
                {
                    if (_trend[index] > 0 && _upperBands.Contains(band.Key))
                    {
                        band.Value[index] = StopLine[index] + bandpts;
                    }
                    else if (_trend[index] < 0 && _lowerBands.Contains(band.Key))
                    {
                        band.Value[index] = StopLine[index] + bandpts;
                    }
                }
            }
        }

        _priorIndex = index;
    }
}
