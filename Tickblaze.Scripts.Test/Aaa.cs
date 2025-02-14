using System.Diagnostics;
using Tickblaze.Scripts.Indicators;

namespace Test;

public partial class Aaa : Indicator
{
    public Aaa()
    {
		Name = "_AAA";
    }

	[Parameter("String Parameter")]
	public string StringParameter { get; set; } = string.Empty;

    public PlotSeries Result { get; set; } = new(Color.Blue);

    protected override void Initialize()
    {
		var atr = new AverageTrueRange();

		Debug.WriteLine(atr);
    }

    protected override void Calculate(int index)
    {
		Result[index] = Bars.Close[index];
    }
}
