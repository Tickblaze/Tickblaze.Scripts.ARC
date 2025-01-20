using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public class Gaps : ChildIndicator
{
	[AllowNull]
    private DrawingPartDictionary<int, Gap> _gaps;

    public IReadOnlyList<Gap> GapList => _gaps;

    public required Color FillColor { get; init; }

    public required ISeries<double> MinHeights { get; init; }

    public int Count => _gaps.Count;

    public bool IsEmpty => _gaps.Count is 0;

    public Gap this[int index] => GetGapAt(index);

    protected override void Initialize()
    {
		if (IsInitialized)
		{
			return;
		}

		_gaps = [];

		IsInitialized = true;
	}

    public Gap GetGapAt(Index index)
    {
        return _gaps.GetDrawingPartAt(index);
    }

    public IEnumerable<Gap> GetVisibleGaps(Rectangle visibleBoundary)
    {
        return _gaps.GetVisibleDrawingParts(visibleBoundary);
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

    public override void OnRender(IDrawingContext drawingContext)
    {
		if (Chart is null || ChartScale is null || RenderTarget is null)
		{
			throw new InvalidOperationException(nameof(OnRender));
		}

        var chartRightX = Chart.GetRightX();

		var boundary = RenderTarget.GetVisibleBoundary();
        var gaps = GetVisibleGaps(boundary);

        foreach (var gap in gaps)
        {
            if (gap.IsEmpty)
            {
                continue;
            }

            var gapStartX = Chart.GetXCoordinateByBarIndex(gap.StartBarIndex);
            var gapStartY = ChartScale.GetYCoordinateByValue(gap.StartPrice);

            var gapEndX = gap.EndBarIndex is null ? chartRightX : Chart.GetXCoordinateByBarIndex(gap.EndBarIndex.Value);
            var gapEndY = ChartScale.GetYCoordinateByValue(gap.EndPrice);

            drawingContext.DrawRectangle(gapStartX, gapStartY, gapEndX, gapEndY, FillColor);
        }
    }
}
