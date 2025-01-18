namespace Tickblaze.Scripts.Arc.Common;

public interface IXPositionable<TThis>
	where TThis : IXPositionable<TThis>
{
	public static abstract bool IsSequential { get; }
}
