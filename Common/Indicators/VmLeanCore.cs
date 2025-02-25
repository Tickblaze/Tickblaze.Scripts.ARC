using System.ComponentModel;

namespace Tickblaze.Community;

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

		CalculateMacdBb(barIndex);

		CalculateSwings();
	}
}
