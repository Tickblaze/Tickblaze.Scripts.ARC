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

    protected override void Calculate(int index)
    {
		Result[index] = Bars.Close[index];
    }
}
