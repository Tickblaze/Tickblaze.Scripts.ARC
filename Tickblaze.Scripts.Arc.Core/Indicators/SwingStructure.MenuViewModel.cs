using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class SwingStructure
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(SwingStructure swingStructure)
		{
			_swingStructure = swingStructure;
		}

		private IDisposable? _swingSubscription;

		private readonly SwingStructure _swingStructure;

		private Swings Swings => _swingStructure._swings;

		public bool ShowSwingLines
		{
			get;
			set
			{
				Swings.ShowLines = _swingStructure.ShowSwingLines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLabels
		{
			get;
			set
			{
				Swings.ShowLabels = _swingStructure.ShowSwingLabels = value;

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

		public void Initialize()
		{
			_swingSubscription?.Dispose();

			MenuHeader = _swingStructure.MenuHeader;

			SwingStrength = _swingStructure.SwingStrength;
			ShowSwingLines = _swingStructure.ShowSwingLines;
			ShowSwingLabels = _swingStructure.ShowSwingLabels;

			_swingSubscription = this
				.WhenAnyValue(viewModel => viewModel.SwingStrength)
				.Throttle(TimeSpan.FromSeconds(0.75), RxApp.TaskpoolScheduler)
				.DistinctUntilChanged()
				.Select(swingStrength => Unit.Default)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(OnSwingStrengthChanged);
		}

		private void OnSwingStrengthChanged(Unit unit)
		{
			_swingStructure.InitializeSwings(true);
		}
	}
}
