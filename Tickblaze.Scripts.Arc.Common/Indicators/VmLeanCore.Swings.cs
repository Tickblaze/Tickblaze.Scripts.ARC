using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Arc.Common;

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

	public void InitializeSwings()
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
			CalculationMode = SwingCalculationMode.CurrentBar,
			SwingDtbDeviation = SwingDeviationAtr.Map(atr => SwingDtbAtrMultiplier * atr),
			SwingDeviation = SwingDeviationAtr.Map(atr => SwingDeviationAtrMultiplier * atr),
		};
	}

	private void CalculateSwings()
	{
		Swings.Calculate();
	}

    public override void OnRender(IDrawingContext drawingContext)
    {
		Swings.OnRender(drawingContext);
    }
}
