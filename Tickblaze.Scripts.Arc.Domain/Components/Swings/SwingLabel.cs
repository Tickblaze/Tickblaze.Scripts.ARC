namespace Tickblaze.Scripts.Arc.Domain;

public sealed class SwingLabel
{
    private SwingLabel(string name, string shortName)
    {
        Name = name;
        ShortName = shortName;
    }

	public string Name { get; }

	public string ShortName { get; }

	public static readonly SwingLabel None = new(string.Empty, string.Empty);

	public static readonly SwingLabel LowerLow = new ("LowerLow", "LL");
	
	public static readonly SwingLabel DoubleTop = new("Double Top", "DT");
	
	public static readonly SwingLabel LowerHigh = new("Lower High", "LH");
	
	public static readonly SwingLabel HigherLow = new ("Higher Low", "HL");
	
	public static readonly SwingLabel HigherHigh = new("Higher High", "HH");
	
	public static readonly SwingLabel DoubleBottom = new("Double Bottom", "DB");
}
