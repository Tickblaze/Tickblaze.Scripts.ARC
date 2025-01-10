using ReactiveUI;

namespace Tickblaze.Scripts.Arc.Common;

public abstract class MenuViewModelBase : ReactiveObject
{
    public MenuViewModelBase(IChartObject chartObject)
    {
        _chartObject = chartObject;

		Changed.Subscribe(OnPropertyChanged);
    }

    private readonly IChartObject _chartObject;
	
	private readonly Capturer _initializeCapturer = new();

    public void Initialize()
	{
		using (_initializeCapturer.Capture())
		{

		}
	}

	private void OnPropertyChanged(IReactivePropertyChangedEventArgs<IReactiveObject> args)
	{
		if (!_initializeCapturer.IsCaptured)
		{
			//_chartObject.OnRender(?);
		}
	}
}
