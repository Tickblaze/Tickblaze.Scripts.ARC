namespace Tickblaze.Scripts.Arc;

public static class ColorExtensions
{
	public static Color ToApiColor(this DrawingColor drawingColor, float? opacity = default)
	{
		var color = Color.FromDrawingColor(drawingColor);

		if (opacity is not null)
		{
			color = Color.New(color, opacity.Value);
		}

		return color;
	}

	public static Color With(this Color color, byte? r = default, byte? g = default, byte? b = default, float? opacity = default)
	{
		byte a = 255;

		if (opacity is not null)
		{
			var aTemporary = Math.Min(255 * opacity ?? 1.0f, 255);

			aTemporary = Math.Max(0, aTemporary);

			a = Convert.ToByte(aTemporary);
		}

		return Color.FromArgb(a, r ?? color.R, g ?? color.G, b ?? color.B);
	}
}
