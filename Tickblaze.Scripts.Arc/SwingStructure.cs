using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tickblaze.Scripts.Arc;

internal partial class SwingStructure : Indicator
{
    public SwingStructure()
    {
    }

    [Parameter("Use Highs/Lows", Description = "Use High/Lows or Input")]
    public bool IsHighLowBased { get; set; }

    [Parameter("Swing strength", Description = "Number of bars used to identify a swing high or low")]
    public int SwingStrength { get; set; }
    
    [Parameter("Show Lines", Description = "Show structure trend lines")]
    public bool ShowSwingLines { get; set; }

    [Parameter("Up-trend line color", Description = "Line color for up-trending structure")]
    public Color UpLineColor { get; set; }

    [Parameter("Down-trend line color", Description = "Line color for down-trending structure")]
    public Color DownLineColor { get; set; }

    [NumericRange(MinValue = 1, MaxValue = 10)]
    [Parameter("Line Thickness", Description = "Thickness of structure lines")]
    public int LineThickness { get; set; }

    [Parameter("Show Labels", Description = "Whether structure labels such as 'HH' and 'LH' need to be shown")]
    public bool ShowSwingLabels { get; set; }

    [Parameter("Label Font", Description = "Font for structure labels")]
    public Font LabelFont { get; set; }

    [Parameter("Menu Header", Description = "Quick access menu header")]
    public string MenuHeader { get; set; }

    protected override void Calculate(int index)
    {
        if (index <= 2)
        {
            return;
        }


    }
}
