namespace Tickblaze.Scripts.Arc.Common;

public sealed class Capturer
{
	private int _captureCount;

	private readonly Lock _lock = new();

	public bool IsCaptured => _captureCount is not 0;

	public Releaser Capture()
	{
		Increment();

		return new(Decrement);
	}

	private void Increment()
	{
		lock (_lock)
		{
			_captureCount++;
		}
	}

	private void Decrement()
	{
		lock (_lock)
		{
			_captureCount = Math.Max(_captureCount - 1, 0);
		}
	}
}
