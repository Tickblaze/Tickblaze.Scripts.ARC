using Tickblaze.Scripts.Arc.Domain;
using Point = Tickblaze.Scripts.Api.Models.Point;
using DomainPoint = Tickblaze.Scripts.Arc.Domain.Point;

namespace Tickblaze.Scripts.Arc;

public static class ChartExtensions
{
	public static Rectangle GetVisibleRectangle(this IChartObject chartObject)
    {
        var (chart, chartScale) = Deconstruct(chartObject);

        return new Rectangle
        {
            ToPrice = chartScale.MaxPrice,
            FromPrice = chartScale.MinPrice,
            ToBarIndex = chart.LastVisibleBarIndex,
            FromBarIndex = chart.FirstVisibleBarIndex,
        };
    }

	public static Point GetPoint(this IChartObject chartObject, DomainPoint point)
	{
		var (chart, chartScale) = Deconstruct(chartObject);

		var xCoordinate = chart.GetXCoordinateByBarIndex(point.BarIndex);
		var yCoordinate = chartScale.GetYCoordinateByValue(point.Price);

		return new(xCoordinate, yCoordinate);
	}

    public static double GetTopYCoordinate(this IChartScale chartScale)
	{
		ArgumentNullException.ThrowIfNull(chartScale);

		return chartScale.GetYCoordinateByValue(chartScale.MaxPrice);
	}

	public static double GetBottomYCoordinate(this IChartScale chartScale)
	{
		ArgumentNullException.ThrowIfNull(chartScale);

		return chartScale.GetYCoordinateByValue(chartScale.MinPrice);
	}

	public static double GetLeftXCoordinate(this IChart chart)
	{
		ArgumentNullException.ThrowIfNull(chart);

		return chart.GetXCoordinateByBarIndex(chart.FirstVisibleBarIndex);
	}

	public static double GetRightXCoordinate(this IChart chart)
	{
		ArgumentNullException.ThrowIfNull(chart);

		return GetLeftXCoordinate(chart) + chart.Width;
	}

	public static Point GetBottomLeftPoint(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetLeftXCoordinate, GetBottomYCoordinate);
	}

	public static Point GetBottomRightPoint(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetRightXCoordinate, GetBottomYCoordinate);
	}

	public static Point GetTopLeftPoint(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetLeftXCoordinate, GetTopYCoordinate);
	}

	public static Point GetTopRightPoint(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetRightXCoordinate, GetTopYCoordinate);
	}

	private static Point GetPoint(IChart? chart, IChartScale? chartScale,
		Func<IChart, double> xCoordinateSelector, Func<IChartScale, double> yCoordinateSelector)
	{
		ArgumentNullException.ThrowIfNull(chart);
		ArgumentNullException.ThrowIfNull(chartScale);

		var xCoordinate = xCoordinateSelector(chart);
		var yCoordinate = yCoordinateSelector(chartScale);

		return new(xCoordinate, yCoordinate);
	}

	private static (IChart, IChartScale) Deconstruct(IChartObject chartObject)
	{
		if (chartObject is not { Chart: not null, ChartScale: not null })
		{
			throw new NullReferenceException();
		}

		return (chartObject.Chart, chartObject.ChartScale);
	}
}
