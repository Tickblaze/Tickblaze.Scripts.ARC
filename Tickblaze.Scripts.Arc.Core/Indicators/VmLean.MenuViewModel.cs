using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(VmLean vmLean)
		{
			_vmLean = vmLean;
		}

		private readonly VmLean _vmLean;

		private IDisposable? _swingSubscription;

		private Swings Swings => _vmLean.Swings;

		public bool ShowSwingLines
        {
            get;
            set
            {
				Swings.ShowLines = _vmLean.ShowSwingLines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLabels
		{
			get;
			set
			{
				Swings.ShowLabels = _vmLean.ShowSwingLabels = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public double SwingDtbAtrMultiplier
		{
			get;
			set
			{
				_vmLean.SwingDtbAtrMultiplier = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public int SwingStrength
		{
			get;
			set
			{
				_vmLean.SwingStrength = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool EnableLevels
        {
            get;
			set
			{
				_vmLean.EnableLevels = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
        }

        public bool ShowLevel1Lines
		{
			get;
			set
			{
				_vmLean.ShowLevel1Lines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowLevel2Lines
		{
			get;
			set
			{
				_vmLean.ShowLevel2Lines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowLevel3Lines
		{
			get;
			set
			{
				_vmLean.ShowLevel3Lines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public string? LevelPlotStyle
		{
			get;
			set
			{
				if (Enum.TryParse<LevelPlotStyle>(value, out var levelPlotStyle))
				{
					_vmLean.LevelPlotStyleValue = levelPlotStyle;
				}

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSentimentBox
		{
			get;
			set
			{
				_vmLean.ShowSentimentBox = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public string? FloodingType
		{
			get;
			set
			{
				if (Enum.TryParse<FloodingType>(value, out var flodingType))
				{
					_vmLean.FloodingTypeValue = flodingType;
				}

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

        [DisallowNull]
		public string? MenuHeader
		{
			get;
			set
			{
				_vmLean.MenuHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

        public void Initialize()
        {
			_swingSubscription?.Dispose();
			
			MenuHeader = _vmLean.MenuHeader;

			SwingStrength = _vmLean.SwingStrength;
			ShowSwingLines = _vmLean.ShowSwingLines;
			ShowSwingLabels = _vmLean.ShowSwingLabels;
			SwingDtbAtrMultiplier = _vmLean.SwingDtbAtrMultiplier;

			EnableLevels = _vmLean.EnableLevels;
			ShowLevel1Lines = _vmLean.ShowLevel1Lines;
			ShowLevel2Lines = _vmLean.ShowLevel2Lines;
			ShowLevel3Lines = _vmLean.ShowLevel3Lines;
			LevelPlotStyle = _vmLean.LevelPlotStyleValue.ToString();

			ShowSentimentBox = _vmLean.ShowSentimentBox;

			FloodingType = _vmLean.FloodingTypeValue.ToString();

			var swingStrengthStream = this
				.WhenAnyValue(viewModel => viewModel.SwingStrength)
				.DistinctUntilChanged()
				.Select(swingStrength => Unit.Default);

			_swingSubscription = this
				.WhenAnyValue(viewModel => viewModel.SwingDtbAtrMultiplier)
				.DistinctUntilChanged()
				.Select(swingDtbAtrMultiplier => Unit.Default)
				.Merge(swingStrengthStream)
				.Throttle(TimeSpan.FromSeconds(0.75), RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Subscribe(OnSwingParameterChanged);
		}

		private void OnSwingParameterChanged(Unit unit)
		{
			_vmLean.ReinitializeSwings();
		}
	}
}
