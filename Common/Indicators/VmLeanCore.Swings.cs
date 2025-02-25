using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Community;

public partial class VmLeanCore
{
	[AllowNull]
	private AverageTrueRange _swingDeviationAtr;

	private ISeries<double> SwingDeviationAtr => _swingDeviationAtr.Result;

	[AllowNull]
	public Swings Swings { get; private set; }

	public int SwingStrength { get; set; } = 3;

	public double SwingDeviationAtrMultiplier { get; set; }

	public double SwingDtbAtrMultiplier { get; set; }

	public void InitializeSwings(bool forceReinitialization)
	{
		_swingDeviationAtr = new AverageTrueRange
		{
			Bars = Bars,
			Period = 256,
			SmoothingType = MovingAverageType.Simple,
		};

		Swings = new Swings
		{
			Bars = Bars,
			RenderTarget = RenderTarget,
			SwingStrength = SwingStrength,
			SwingDtbAtrMultiplier = SwingDtbAtrMultiplier,
			CalculationMode = SwingCalculationMode.CurrentBar,
			SwingDeviationAtrMultiplier = SwingDeviationAtrMultiplier,
		};

		if (forceReinitialization)
		{
			_swingDeviationAtr.Reinitialize(this);

			Swings.Reinitialize(this);
		}
	}

	public void CalculateSwings()
	{
		Swings.Calculate();
	}

    public override void OnRender(IDrawingContext drawingContext)
    {
		Swings.OnRender(drawingContext);
    }
}
