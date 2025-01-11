
namespace Tickblaze.Scripts.Arc.Core;

// Todo: separate paremeters and their visuals.
public partial class VmLean : Indicator
{
	public VmLean()
	{
		ShortName = "TBC VML";
		Name = "TB Core VM Lean";
	}

	[Parameter("Settings Header", Description = "Quick access settings header")]
	public string SettingsHeader { get; set; } = "VM Lean";

	[Plot("Zero Line")]
	public PlotSeries ZeroLine { get; set; } = new(Color.Black, LineStyle.Solid, 2);

    protected override Parameters GetParameters(Parameters parameters)
    {
		HideSwingParameters(parameters);

		HidePriceExcursionParameters(parameters);

		return parameters;
    }

    protected override void Initialize()
    {
		InitializeSwings();

		InitializeMacdBb();

		InitializeHistogram();

		InitializeFlooding();
	}

    protected override void Calculate(int barIndex)
    {
		ZeroLine[barIndex] = 0.0d;

		CalculateHistogram(barIndex);

		CalculateMacdBb(barIndex);

		CalculateSwings(barIndex);

		CalculateFlooding(barIndex);
    }

    public override void OnRender(IDrawingContext drawingContext)
    {
		RenderFlooding(drawingContext);

		RenderPriceExcursions(drawingContext);
    }
}
