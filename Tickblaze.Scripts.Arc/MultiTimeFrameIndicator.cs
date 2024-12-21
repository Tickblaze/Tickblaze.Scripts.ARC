namespace Tickblaze.Scripts.Indicators;

public partial class SampleMultiSymbolIndicator : Indicator
{
    [Plot("Result")]
    public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);
    private BarSeries _additionalSeries;
    protected override void Initialize()
    {
        var symbol = GetSymbol(new FindSymbolRequest
        {
            SymbolCode = "NQ",
            Contract = new BarSeriesContract { Type = BarSeriesContractType.ContinuousByDataProvider },
            ExchangeCode = "CMES",
            InstrumentType = SymbolType.Future,
            IsETH = true
        });
        if (symbol == null)
            return;
        _additionalSeries = GetBarSeries(new BarSeriesRequest
        {
            Period = Bars.Period,
            Symbol = symbol,
            BarSeriesContract = new BarSeriesContract { Type = BarSeriesContractType.ContinuousByDataProvider }
        });
    }
    protected override void Calculate(int index)
    {
        if (_additionalSeries is not { Count: > 0 })
            return;
        Result[index] = _additionalSeries.Close[^1];
    }
}

public partial class SampleMtfIndicator : Indicator
{
    [Plot("Result")]
    public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);
    private BarSeries _2MinuteSeries;
    protected override void Initialize()
    {
        _2MinuteSeries = GetBarSeries(new BarSeriesRequest
        {
            Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 2),
            Symbol = Bars.Symbol,
            BarSeriesContract = new BarSeriesContract { Type = BarSeriesContractType.ContinuousBackAdjusted }
        });
    }
    protected override void Calculate(int index)
    {
        if (_2MinuteSeries is not { Count: > 0 })
            return;
        Result[index] = _2MinuteSeries.Close[^1];
    }
}