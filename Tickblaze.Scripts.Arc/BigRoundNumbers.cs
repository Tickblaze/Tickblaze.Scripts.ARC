
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// ARC_BigRoundNumbers [ABRN]
/// </summary>

public partial class BigRoundNumbers : Indicator
{
    [Parameter("Base Price"), NumericRange(0, double.MaxValue)]
    public double BasePrice { get; set; } = 0;

    [Parameter("Interval Size"), NumericRange(0, double.MaxValue)]
    public double IntervalSize { get; set; } = 5;

    [Parameter("Interval Price")]
    public IntervalType Interval { get; set; } = IntervalType.Points;

    [Plot("L1")]
    public PlotSeries Line1 { get; set; } = new(Color.Red);

    [Plot("L2")]
    public PlotSeries Line2 { get; set; } = new(Color.Red);

    [Plot("L3")]
    public PlotSeries Line3 { get; set; } = new(Color.Red);

    [Plot("L4")]
    public PlotSeries Line4 { get; set; } = new(Color.Blue);

    [Plot("L5")]
    public PlotSeries Line5 { get; set; } = new(Color.Blue);

    [Plot("L6")]
    public PlotSeries Line6 { get; set; } = new(Color.Blue);

    public enum IntervalType
    {
        Points,
        Ticks,
        Pips
    };

    private double _intervalPoints;
    private double _basePriceCalibrated = double.MinValue;
    private int _priorIndex = -1;
    private Dictionary<int, double> _values = new();

    public BigRoundNumbers()
    {
        Name = "ARC BigRoundNumbers";
        ShortName = "ABRN";
        IsOverlay = true;
    }

    protected override void Initialize()
    {
        _values[1] = 0;
        _values[2] = 0;
        _values[3] = 0;
        _values[4] = 0;
        _values[5] = 0;
        _values[6] = 0;

        if (Interval == IntervalType.Points)
        {
            _intervalPoints = IntervalSize < Bars.Symbol.TickSize * 2 ? Bars.Symbol.TickSize * 2 : IntervalSize;
        }
        else if (Interval == IntervalType.Ticks)
        {
            _intervalPoints = Math.Max(2, IntervalSize) * Bars.Symbol.TickSize;
        }
        else if (Interval == IntervalType.Pips)
        {
            _intervalPoints = Math.Max(1, IntervalSize) * 10 * Bars.Symbol.TickSize;
        }
    }

    protected override void Calculate(int index)
    {
        var price = _basePriceCalibrated;

        if (Bars[index].Close > _values[2] || Bars[index].Close < _values[5])
        {
            _basePriceCalibrated = double.MinValue;
        }

        if (_basePriceCalibrated == double.MinValue)
        {
            price = BasePrice;
            if (price > Bars[index].Close)
            {
                while (price > Bars[index].Close)
                {
                    price -= _intervalPoints;
                }

                price += _intervalPoints;
            }
            else
            {
                while (price < Bars[index].Close)
                {
                    price += _intervalPoints;
                }
            }

            _basePriceCalibrated = price;
            _values[3] = price;
            _values[2] = _values[3] + _intervalPoints;
            _values[1] = _values[2] + _intervalPoints;
            _values[4] = price - _intervalPoints;
            _values[5] = _values[4] + -_intervalPoints;
            _values[6] = _values[5] - _intervalPoints;
        }

        Line1[index] = _values[1];
        Line2[index] = _values[2];
        Line3[index] = _values[3];
        Line4[index] = _values[4];
        Line5[index] = _values[5];
        Line6[index] = _values[6];
    }
    public override void OnRender(IDrawingContext context)
    {
        if (ChartScale == null || Chart == null)
        {
            return;
        }

        var index = Bars.Count - 1;
        if (index != _priorIndex)
        {
            _priorIndex = index;
            _basePriceCalibrated = double.MinValue;
        }

        if (_basePriceCalibrated == double.MinValue)
        {
            var price = BasePrice;
            if (price > Bars[index].Close)
            {
                while (price > Bars[index].Close)
                {
                    price -= _intervalPoints;
                }

                price += _intervalPoints;
            }
            else
            {
                while (price < Bars[index].Close)
                {
                    price += _intervalPoints;
                }
            }

            _basePriceCalibrated = price;
        }

        var maxPrice = ChartScale.GetValueByYCoordinate(0);
        var minPrice = ChartScale.GetValueByYCoordinate(Chart.Height);

        var pointL = new Point(0, 0);
        var pointR = new Point(Chart.Width, 0);
        var priceLevel = _basePriceCalibrated;

        while (priceLevel < maxPrice)
        {
            priceLevel += _intervalPoints;
        }

        priceLevel -= _intervalPoints;

        while (priceLevel > minPrice)
        {
            pointL.Y = pointR.Y = ChartScale.GetYCoordinateByValue(priceLevel);
            context.DrawLine(pointL, pointR, Plots[0].Color, Plots[0].Thickness);
            priceLevel -= _intervalPoints;
        }
    }
}