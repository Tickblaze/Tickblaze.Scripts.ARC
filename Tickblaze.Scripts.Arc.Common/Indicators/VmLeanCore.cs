using System.ComponentModel;

namespace Tickblaze.Scripts.Arc.Common;

[Browsable(false)]
public partial class VmLeanCore : ChildIndicator
{
	protected override void Initialize()
	{
		if (IsInitialized)
		{
			return;
		}

		InitializeHistogram();

		InitializeMacdBb();
		
		InitializeSwings(false);

		IsInitialized = true;
	}

	protected override void Calculate(int barIndex)
	{
		CalculateHistogram(barIndex);

		CalculateMacdBb();

		CalculateSwings();
	}
}
