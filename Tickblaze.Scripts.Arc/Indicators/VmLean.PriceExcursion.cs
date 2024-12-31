namespace Tickblaze.Scripts.Arc;

public partial class VmLean
{
	[Parameter("Level Line Style", GroupName = "Price Excursion Parameters", Description = "Style of level lines")]
	public LineStyle LevelLineStyle { get; set; } = LineStyle.Solid;

	[NumericRange(MinValue = 1)]
	[Parameter("Level Line Thickness", GroupName = "Price Excursion Parameters", Description = "Thickness of level lines")]
	public int LevelLineThickness { get; set; } = 3;

	[Parameter("Show Level 1", GroupName = "Price Excursion Parameters", Description = "Whether level 1 is shown")]
	public bool ShowLevel1 { get; set; }

	[Parameter("Level 1 Color", GroupName = "Price Excursion Parameters")]
	public Color Level1Color { get; set; } = DrawingColor.WhiteSmoke;

	[Parameter("Show Level 2", GroupName = "Price Excursion Parameters", Description = "Whether level 2 is shown")]
	public bool ShowLevel2 { get; set; }

	[Parameter("Level 2 Color", GroupName = "Price Excursion Parameters")]
	public Color Level2Color { get; set; } = Color.Blue;

	[Parameter("Show Level 3", GroupName = "Price Excursion Parameters", Description = "Whether level 3 is shown")]
	public bool ShowLevel3 { get; set; }
	
	[Parameter("Level 3 Color", GroupName = "Price Excursion Parameters")]
	public Color Level3Color { get; set; } = Color.Red;
}
