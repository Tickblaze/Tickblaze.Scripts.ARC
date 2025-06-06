﻿namespace Tickblaze.Community;

public class DrawingPartSet<TBoundable> : DrawingPartDictionary<TBoundable, DrawingPart<TBoundable>>
	where TBoundable : notnull, IBoundable, IXPositionable<TBoundable>, IEquatable<TBoundable>
{
	public void AddOrUpdate(TBoundable boundable)
	{
		var drawingPart = boundable.ToDrawingPart();

		AddOrUpdate(drawingPart);
	}
}
