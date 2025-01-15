namespace Tickblaze.Scripts.Arc.Common;

public class DrawingPartSet<TBoundable> : DrawingPartDictionary<TBoundable, DrawingPart<TBoundable>>
	where TBoundable : notnull, IBoundable, IEquatable<TBoundable>
{
	public void AddOrUpdate(TBoundable boundable)
	{
		var drawingPart = boundable.ToDrawingPart();

		AddOrUpdate(drawingPart);
	}
}
