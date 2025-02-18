namespace Tickblaze.Community;

public interface IXPositionable<TThis>
	where TThis : IXPositionable<TThis>
{
	public static abstract bool IsSequential { get; }
}
