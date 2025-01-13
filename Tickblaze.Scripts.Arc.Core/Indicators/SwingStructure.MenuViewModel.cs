using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

namespace Tickblaze.Scripts.Arc.Core;

public partial class SwingStructure
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(SwingStructure swingStructure)
		{
			_swingStructure = swingStructure;

			//this.WhenAnyValue(viewModel => viewModel.SwingStrength)
			//	.Throttle(TimeSpan.FromSeconds(0.75), RxApp.TaskpoolScheduler)
			//	.DistinctUntilChanged()
			//	.ObserveOn(RxApp.MainThreadScheduler)
			//	.Subscribe(_ => _swingStructure.Initialize());
		}

		private readonly SwingStructure _swingStructure;

		public bool ShowSwingLines
		{
			get;
			set
			{
				_swingStructure.ShowSwingLines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLabels
		{
			get;
			set
			{
				_swingStructure.ShowSwingLabels = field;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public int SwingStrength
		{
			get;
			set
			{
				_swingStructure.SwingStrength = field;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		[DisallowNull]
		public string? MenuHeader
		{
			get;
			private set
			{
				_swingStructure.MenuHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public void Initialize()
		{
			MenuHeader = _swingStructure.MenuHeader;

			SwingStrength = _swingStructure.SwingStrength;
			ShowSwingLines = _swingStructure.ShowSwingLines;
			ShowSwingLabels = _swingStructure.ShowSwingLabels;
		}
	}
}
