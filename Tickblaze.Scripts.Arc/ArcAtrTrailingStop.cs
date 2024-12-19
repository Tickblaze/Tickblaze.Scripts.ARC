
using Tickblaze.Scripts.Api.Enums;

namespace Tickblaze.Scripts.Arc;

/// <summary>
/// ARC ATRTrailingStop [AATS]
/// </summary>
public partial class ArcAtrTrailingStop : Indicator
{
    [Parameter("ATR Period"), NumericRange(1, int.MaxValue)]
    public int ATRPeriod { get; set; } = 14;

    [Parameter("ATR Multiplier"), NumericRange(0.01, double.MaxValue)]
    public double ATRMultiplier { get; set; } = 2.5;

    [Parameter("ATR Band Period"), NumericRange(1, int.MaxValue)]
    public int ATRBandPeriod { get; set; } = 14;

    [Parameter("Round band to tick")]
    public bool RoundBandToTick { get; set; } = true;

    [Parameter("Show opposite bands")]
    public bool ShowOppositeBands { get; set; } = true;

    [Parameter("Band 1 Multiplier"), NumericRange(0, double.MaxValue)]
    public double Band1Multiplier { get; set; } = 1;

    [Parameter("Band 2 Multiplier"), NumericRange(0, double.MaxValue)]
    public double Band2Multiplier { get; set; } = 2;

    [Parameter("Band 3 Multiplier"), NumericRange(0, double.MaxValue)]
    public double Band3Multiplier { get; set; } = 3;

    [Plot("Stop Dot")]
    public PlotSeries StopDot { get; set; } = new("#808080", PlotStyle.Dot);

    [Plot("Stop Line")]
    public PlotSeries StopLine { get; set; } = new("#808080", PlotStyle.Hash);

    [Plot("Reverse Dot")]
    public PlotSeries ReverseDot { get; set; } = new("#000000", PlotStyle.Dot);

    [Plot("BandEdge U3")]
    public PlotSeries BandEdgeU3 { get; set; } = new(Color.Red, PlotStyle.Hash);

    [Plot("BandEdge U2")]
    public PlotSeries BandEdgeU2 { get; set; } = new(Color.Red, PlotStyle.Hash);

    [Plot("BandEdge U1")]
    public PlotSeries BandEdgeU1 { get; set; } = new(Color.Red, PlotStyle.Hash);

    [Plot("BandEdge L1")]
    public PlotSeries BandEdgeL1 { get; set; } = new(Color.Cyan, PlotStyle.Hash);

    [Plot("BandEdge L2")]
    public PlotSeries BandEdgeL2 { get; set; } = new(Color.Cyan, PlotStyle.Hash);

    [Plot("BandEdge L3")]
    public PlotSeries BandEdgeL3 { get; set; } = new(Color.Cyan, PlotStyle.Hash);

    private AverageTrueRange _offsetSeries;
    private DataSeries _preliminaryTrend, _trend, _currentStopLong, _currentStopShort;
    private int _regionID;
    private int _priorIndex = -1;
    private Dictionary<BandType, PlotSeries> _bandPlots = [];
    private Dictionary<BandType, double> _bandMults = [];
    private List<BandType> _upperBands = [];
    private List<BandType> _lowerBands = [];

    private enum BandType
    {
        L1,
        L2,
        L3,
        U1,
        U2,
        U3
    }

    public ArcAtrTrailingStop()
    {
        Name = "ARC ATRTrailingStop";
        ShortName = "AATS";
        IsOverlay = true;
    }

    protected override void Initialize()
    {
        _preliminaryTrend = new DataSeries();
        _trend = new DataSeries();
        _currentStopLong = new DataSeries();
        _currentStopShort = new DataSeries();
        _offsetSeries = new AverageTrueRange(ATRPeriod, MovingAverageType.Simple);

        _bandPlots[BandType.L1] = BandEdgeL1;
        _bandPlots[BandType.U1] = BandEdgeU1;
        _bandPlots[BandType.L2] = BandEdgeL2;
        _bandPlots[BandType.U2] = BandEdgeU2;
        _bandPlots[BandType.L3] = BandEdgeL3;
        _bandPlots[BandType.U3] = BandEdgeU3;

        _bandMults[BandType.L1] = -Band1Multiplier;
        _bandMults[BandType.U1] = Band1Multiplier;
        _bandMults[BandType.L2] = -Band2Multiplier;
        _bandMults[BandType.U2] = Band2Multiplier;
        _bandMults[BandType.L3] = -Band3Multiplier;
        _bandMults[BandType.U3] = Band3Multiplier;

        _upperBands.Add(BandType.U1);
        _upperBands.Add(BandType.U2);
        _upperBands.Add(BandType.U3);
        _lowerBands.Add(BandType.L1);
        _lowerBands.Add(BandType.L2);
        _lowerBands.Add(BandType.L3);
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

        var trailingAmount = ATRMultiplier * Math.Max(Bars.Symbol.TickSize, _offsetSeries[index - 1]);
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

    private static int ToTime(DateTime t)
    {
        return t.Hour * 10000 + t.Minute * 100 + t.Second;
    }
}
