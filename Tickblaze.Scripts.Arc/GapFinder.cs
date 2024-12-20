using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc;

public partial class GapFinder : Indicator
{
    public GapFinder()
    {
        IsOverlay = true;
    }

    private AverageTrueRange _averageTrueRange = default!;

    private readonly List<Gap> _freshFvgs = [];
    private readonly List<Gap> _testedFvgs = [];
    private readonly List<Gap> _brokenFvgs = [];

    [Parameter("Measurement", GroupName = "Parameters")]
    public GapMeasurement GapMeasurementValue { get; set; } = GapMeasurement.Atr;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Ticks", GroupName = "Parameters")]
    public int GapTickCount { get; set; } = 8;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Pts", GroupName = "Parameters")]
    public int GapPointCount { get; set; } = 5;

    [NumericRange(MinValue = 1)]
    [Parameter("Gap Pts", GroupName = "Parameters")]
    public int GapPipCount { get; set; } = 20;

    [NumericRange(MinValue = 0.01, MaxValue = double.MaxValue)]
    [Parameter("Gap ATR Multiple", GroupName = "Parameters")]
    public double AtrMultiple { get; set; } = 0.5;

    [NumericRange(MinValue = 1)]
    [Parameter("ATR Period", GroupName = "Parameters")]
    public int AtrPeriod { get; set; } = 14;

    [Parameter("Restrict to New Session", GroupName = "Parameters")]
    public bool IsRestrictedToNewSessions { get; set; }

    [Parameter("Show Fresh Gaps", GroupName = "Visuals")]
    public bool ShowFreshGaps { get; set; }

    [Parameter("Fresh Gap Color", GroupName = "Visuals")]
    public Color FreshGapColor { get; set; } = Color.New(Color.Orange, 0.5f);

    [Parameter("Show Tested Gaps", GroupName = "Visuals")]
    public bool ShowTestedGaps { get; set; }

    [Parameter("Tested Gap Color", GroupName = "Visuals")]
    public Color TestedGapColor { get; set; } = Color.New(Color.Silver, 0.5f);

    [Parameter("Show Broken Gaps", GroupName = "Visuals")]
    public bool ShowBrokenGaps { get; set; }

    [Parameter("Broken Gap Color", GroupName = "Visuals")]
    public Color BrokenGapColor { get; set; } = Color.New(Color.DimGray, 0.5f);

    [Parameter("Button Text", GroupName = "Visuals")]
    public string ButtonText { get; set; } = "GapFinder";

    [Parameter("Enable Sounds", GroupName = "Alerts")]
    public bool AreSoundEnabled { get; set; }

    [Parameter("Gap Hit WAV", GroupName = "Alerts")]
    public string? GapHitWav { get; set; }

    protected override Parameters GetParameters(Parameters parameters)
    {
        List<string> gapSizePropertyNames =
        [
            nameof(GapTickCount),
            nameof(GapPointCount),
            nameof(GapPipCount),
            nameof(AtrMultiple),
            nameof(AtrPeriod),
        ];

        if (!ShowFreshGaps)
        {
            parameters.Remove(nameof(FreshGapColor));
        }

        if (!ShowTestedGaps)
        {
            parameters.Remove(nameof(TestedGapColor));
        }

        if (!ShowBrokenGaps)
        {
            parameters.Remove(nameof(BrokenGapColor));
        }

        return parameters;
    }
}
