using System.ComponentModel.DataAnnotations;

namespace Tickblaze.Scripts.Arc;

public partial class VmLean : Indicator
{
	public VmLean()
	{
		ShortName = "TBC VML";
		Name = "TB Core VM Lean";
	}

	[Parameter("Optimization Mode", Description = "Optimization Mode improves run-time performance by reducing the number of historical chart markers")]
	public OptimizationMode OptimizationModeValue { get; set; } = OptimizationMode.Maximum;

	[Parameter("Settings Header", Description = "Quick access settings header")]
	public string SettingsHeader { get; set; } = "VM Lean";

	public PlotSeries ZeroLine { get; set; } = new(Color.Black);
}
