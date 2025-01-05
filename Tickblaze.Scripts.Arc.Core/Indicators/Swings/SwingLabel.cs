namespace Tickblaze.Scripts.Arc.Core;

public sealed class SwingLabel
{
    private SwingLabel(int id, string name, string shortName)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
    }

	private int Id { get; }

	public string Name { get; }

	public string ShortName { get; }

	public bool IsLowerLow => Id is 1;

	public bool IsDoubleTop => Id is 2;

	public bool IsLowerHigh => Id is 3;

	public bool IsHigherLow => Id is 4;

	public bool IsHigherHigh => Id is 5;

	public bool IsDoubleBottom => Id is 6;

	public bool IsLow => IsHigherLow || IsLowerLow || IsDoubleBottom;

	public bool IsHigh => IsHigherHigh || IsLowerHigh || IsDoubleTop;

	public static readonly SwingLabel None = new(0, string.Empty, string.Empty);

	public static readonly SwingLabel LowerLow = new (1, "Lower Low", "LL");
	
	public static readonly SwingLabel DoubleTop = new(2, "Double Top", "DT");
	
	public static readonly SwingLabel LowerHigh = new(3, "Lower High", "LH");
	
	public static readonly SwingLabel HigherLow = new(4, "Higher Low", "HL");
	
	public static readonly SwingLabel HigherHigh = new(5, "Higher High", "HH");
	
	public static readonly SwingLabel DoubleBottom = new(6, "Double Bottom", "DB");
}
