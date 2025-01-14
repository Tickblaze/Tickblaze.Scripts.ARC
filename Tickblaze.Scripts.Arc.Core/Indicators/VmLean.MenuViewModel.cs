using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using Tickblaze.Scripts.Arc.Common;

namespace Tickblaze.Scripts.Arc.Core;

public partial class VmLean
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(VmLean vmLean)
		{
			_vmLean = vmLean;

			_swings = _vmLean._swings;

			SwingStrength = _vmLean.SwingStrength;
			IsSwingEnabled = _vmLean.IsSwingEnabled;
			ShowSwingLines = _vmLean.ShowSwingLines;
			ShowSwingLabels = _vmLean.ShowSwingLabels;
			SwingDtbAtrMultiplier = _vmLean.SwingDtbAtrMultiplier;

			ShowLevel1 = _vmLean.ShowLevel1;
			ShowLevel2 = _vmLean.ShowLevel2;
			ShowLevel3 = _vmLean.ShowLevel3;
			EnableLevels = _vmLean.EnableLevels;
			LevelPlotStyle = _vmLean.LevelPlotStyleValue;

			ShowSentimentBox = _vmLean.ShowSentimentBox;

			FloodingType = _vmLean.FloodingTypeValue;

			MenuHeader = _vmLean.MenuHeader;
		}

		private Swings _swings;

		private readonly VmLean _vmLean;

		public bool IsSwingEnabled
		{
			get;
			set
			{
				_swings.IsSwingEnabled = _vmLean.IsSwingEnabled = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLines
        {
            get;
            set
            {
				_swings.ShowLines = _vmLean.ShowSwingLines = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowSwingLabels
		{
			get;
			set
			{
				_swings.ShowLabels = _vmLean.ShowSwingLabels = value;

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

        public bool ShowLevel1
		{
			get;
			set
			{
				_vmLean.ShowLevel1 = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowLevel2
		{
			get;
			set
			{
				_vmLean.ShowLevel2 = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowLevel3
		{
			get;
			set
			{
				_vmLean.ShowLevel3 = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public LevelPlotStyle LevelPlotStyle
		{
			get;
			set
			{
				_vmLean.LevelPlotStyleValue = value;

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

		public FloodingType FloodingType
		{
			get;
			set
			{
				_vmLean.FloodingTypeValue = value;

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
	}
}
