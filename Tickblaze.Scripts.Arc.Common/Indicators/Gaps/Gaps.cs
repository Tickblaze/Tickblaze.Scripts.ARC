using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Gaps : ChildIndicator
{
    private readonly DrawingPartDictionary<int, Gap> _gaps = [];

    public IReadOnlyList<Gap> GapList => _gaps;

    public required Color FillColor { get; init; }

    public required ISeries<double> MinHeights { get; init; }

    public int Count => _gaps.Count;

    public bool IsEmpty => _gaps.Count is 0;

    public Gap this[int index] => GetGapAt(index);

    public bool Contains(Gap gap)
    {
        return _gaps.Contains(gap);
    }

    public bool TryGetGap(int startBarIndex, [MaybeNullWhen(false)] out Gap gap)
    {
        return _gaps.TryGetDrawingPart(startBarIndex, out gap);
    }

    public Gap GetGapAt(Index index)
    {
        return _gaps.GetDrawingPartAt(index);
    }

    public IEnumerable<Gap> GetVisibleGaps(Rectangle visibleBoundary)
    {
        return _gaps.GetVisibleDrawingParts(visibleBoundary);
    }

    public int IndexOf(Gap gap)
    {
        return _gaps.IndexOf(gap.Key);
    }

	public void AddGaps(int barIndex, params IEnumerable<Gap> gaps)
    {
		var minGapHeight = MinHeights[barIndex];

		foreach (var gap in gaps)
		{
			if (gap.EndPrice - gap.StartPrice > minGapHeight)
			{
				AddOrUpdate(gap);
			}
		}
	}

	public void AddOrUpdate(Gap gap)
	{
		_gaps.AddOrUpdate(gap);
	}

	public bool Remove(Gap gap)
    {
        return _gaps.Remove(gap);
    }

    public bool Remove(int startBarIndex)
    {
        return _gaps.Remove(startBarIndex);
    }

    public void RemoveAt(int index)
    {
        _gaps.RemoveAt(index);
    }

    public void Clear()
    {
        _gaps.Clear();
    }

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (Chart is null
			|| ChartScale is null
			|| RenderTarget is null)
		{
			throw new InvalidOperationException(nameof(OnRender));
		}

		var visibleBoundary = RenderTarget.GetVisibleBoundary();

        var chartRightX = Chart.GetRightX();
        var visibleGaps = GetVisibleGaps(visibleBoundary);

        foreach (var visibleGap in visibleGaps)
        {
            if (visibleGap.IsEmpty)
            {
                continue;
            }

            var gapStartX = Chart.GetXCoordinateByBarIndex(visibleGap.StartBarIndex);
            var gapStartY = ChartScale.GetYCoordinateByValue(visibleGap.StartPrice);

            var gapEndX = visibleGap.EndBarIndex is null
                ? chartRightX : Chart.GetXCoordinateByBarIndex(visibleGap.EndBarIndex.Value);
            var gapEndY = ChartScale.GetYCoordinateByValue(visibleGap.EndPrice);

            drawingContext.DrawRectangle(gapStartX, gapStartY, gapEndX, gapEndY, FillColor);
        }
    }
}
