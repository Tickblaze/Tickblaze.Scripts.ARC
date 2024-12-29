using Tickblaze.Scripts.Arc.Domain;
using Point = Tickblaze.Scripts.Api.Models.Point;

namespace Tickblaze.Scripts.Arc;

public static class RenderingExtensions
{
	public static void DrawRectangle(this IDrawingContext drawingContext
		, double startX, double startY, double endX, double endY, Color? fillColor
		, Color? lineColor = default, int lineThickness = 1, LineStyle lineStyle = LineStyle.Solid)
	{
		ArgumentNullException.ThrowIfNull(drawingContext);

		var topLeft = new Point(startX, startY);
		var bottomRight = new Point(endX, endY);

		drawingContext.DrawRectangle(topLeft, bottomRight, fillColor, lineColor, lineThickness, lineStyle);
	}

	public static void DrawLine(this IDrawingContext drawingContext
		, double startX, double startY, double endX, double endY, Color lineColor
		, int lineThickness = 1, LineStyle lineStyle = LineStyle.Solid)
	{
		ArgumentNullException.ThrowIfNull(drawingContext);

		var topLeft = new Point(startX, startY);
		var bottomRight = new Point(endX, endY);

		drawingContext.DrawLine(topLeft, bottomRight, lineColor, lineThickness, lineStyle);
	}

	public static void DrawHorizontalLine(this IDrawingContext drawingContext
		, double startX, double startY, double endX, Color lineColor
		, int lineThickness = 1, LineStyle lineStyle = LineStyle.Solid)
	{
		DrawLine(drawingContext, startX, startY, endX, startY, lineColor, lineThickness, lineStyle);
	}

	public static void DrawHorizontalRay(this IDrawingContext drawingContext
		, double rayX, double rayY, HorizontalDirection horizontalDirection
		, Color color, int thickness = 1, LineStyle lineStyle = LineStyle.Solid)
	{
		ArgumentNullException.ThrowIfNull(drawingContext);

		var fromPoint = new Point(rayX, rayY);
		var toPoint = new Point(rayX, rayY + horizontalDirection.GetSign());

		drawingContext.DrawRay(fromPoint, toPoint, color, thickness, lineStyle);
	}

	public static void DrawText(this IDrawingContext drawingContext
		, double textX, double textY, string text, Color color, Font? font = default)
	{
		ArgumentNullException.ThrowIfNull(drawingContext);
		ArgumentNullException.ThrowIfNull(text);

		var textPoint = new Point(textX, textY);

		drawingContext.DrawText(textPoint, text, color, font);
	}
}
