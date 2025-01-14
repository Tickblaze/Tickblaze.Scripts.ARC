using ReactiveUI;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class SwingStructure
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(SwingStructure swingStructure)
		{
			_swings = swingStructure._swings;

			_swingStructure = swingStructure;

			MenuHeader = _swingStructure.MenuHeader;

			SwingStrength = _swingStructure.SwingStrength;
			ShowSwingLines = _swingStructure.ShowSwingLines;
			ShowSwingLabels = _swingStructure.ShowSwingLabels;

			this.WhenAnyValue(viewModel => viewModel.SwingStrength)
				.Throttle(TimeSpan.FromSeconds(0.75), RxApp.TaskpoolScheduler)
				.DistinctUntilChanged()
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(_ => OnSwingStrengthChanged());
		}

        private Swings _swings;

		private readonly SwingStructure _swingStructure;

		public bool ShowSwingLines
		{
			get;
			set
			{
				_swings.ShowLines = _swingStructure.ShowSwingLines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLabels
		{
			get;
			set
			{
				_swings.ShowLabels = _swingStructure.ShowSwingLabels = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public int SwingStrength
		{
			get;
			set
			{
				_swingStructure.SwingStrength = value;

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

		private void OnSwingStrengthChanged()
		{
			_swings = _swingStructure.InitializeSwings(true);
		}
	}
}
