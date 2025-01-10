namespace Tickblaze.Scripts.Arc.Common;

public sealed class Releaser : IDisposable
{
    public Releaser(Action releaseAction)
    {
        _releaseAction = releaseAction;
    }

    private readonly Action _releaseAction;

    public void Dispose()
    {
		_releaseAction();
	}
}