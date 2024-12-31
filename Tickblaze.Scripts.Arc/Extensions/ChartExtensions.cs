using Tickblaze.Scripts.Arc.Domain;
using Point = Tickblaze.Scripts.Api.Models.Point;
using DomainPoint = Tickblaze.Scripts.Arc.Domain.Point;

namespace Tickblaze.Scripts.Arc;

public static class ChartExtensions
{
	public static double GetAbsoluteBarWidth(this IChart chart)
	{
		ArgumentNullException.ThrowIfNull(chart);

		var startBarX = chart.GetXCoordinateByBarIndex(0);
		var endBarX = chart.GetXCoordinateByBarIndex(1);

		return endBarX - startBarX;
	}

	public static Rectangle GetVisibleRectangle(this IChartObject chartObject)
    {
        var (chart, chartScale) = Deconstruct(chartObject);

        return new Rectangle
        {
            EndPrice = chartScale.MaxPrice,
            StartPrice = chartScale.MinPrice,
            EndBarIndex = chart.LastVisibleBarIndex,
            StartBarIndex = chart.FirstVisibleBarIndex,
        };
    }

	public static Point ToApiPoint(this IChartObject chartObject, DomainPoint point)
	{
		var (chart, chartScale) = Deconstruct(chartObject);

		var pointX = chart.GetXCoordinateByBarIndex(point.BarIndex);
		var pointY = chartScale.GetYCoordinateByValue(point.Price);

		return new(pointX, pointY);
	}

    public static double GetTopY(this IChartScale chartScale)
	{
		ArgumentNullException.ThrowIfNull(chartScale);

		return chartScale.GetYCoordinateByValue(chartScale.MaxPrice);
	}

	public static double GetBottomY(this IChartScale chartScale)
	{
		ArgumentNullException.ThrowIfNull(chartScale);

		return chartScale.GetYCoordinateByValue(chartScale.MinPrice);
	}

	public static double GetLeftX(this IChart chart)
	{
		ArgumentNullException.ThrowIfNull(chart);

		return chart.GetXCoordinateByBarIndex(chart.FirstVisibleBarIndex);
	}

	public static double GetRightX(this IChart chart)
	{
		ArgumentNullException.ThrowIfNull(chart);

		return GetLeftX(chart) + chart.Width;
	}

	public static Point GetBottomLeft(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetLeftX, GetBottomY);
	}

	public static Point GetBottomRight(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetRightX, GetBottomY);
	}

	public static Point GetTopLeft(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetLeftX, GetTopY);
	}

	public static Point GetTopRight(this IChartObject chartObject)
	{
		ArgumentNullException.ThrowIfNull(chartObject);

		return GetPoint(chartObject.Chart, chartObject.ChartScale, GetRightX, GetTopY);
	}

	private static Point GetPoint(IChart? chart, IChartScale? chartScale,
		Func<IChart, double> xCoordinateSelector, Func<IChartScale, double> yCoordinateSelector)
	{
		ArgumentNullException.ThrowIfNull(chart);
		ArgumentNullException.ThrowIfNull(chartScale);

		var pointX = xCoordinateSelector(chart);
		var pointY = yCoordinateSelector(chartScale);

		return new(pointX, pointY);
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
