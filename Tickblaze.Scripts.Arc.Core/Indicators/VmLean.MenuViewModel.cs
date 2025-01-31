using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

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

		public string? FloodingType
		{
			get;
			set
			{
				if (Enum.TryParse<FloodingType>(value, out var flodingType)
					&& !flodingType.EnumEquals(_vmLean.FloodingTypeValue))
                {
					_vmLean.UpdateFloodingType(flodingType);
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
			MenuHeader = _vmLean.MenuHeader;

			EnableLevels = _vmLean.EnableLevels;
			ShowLevel1Lines = _vmLean.ShowLevel1Lines;
			ShowLevel2Lines = _vmLean.ShowLevel2Lines;
			ShowLevel3Lines = _vmLean.ShowLevel3Lines;
			LevelPlotStyle = _vmLean.LevelPlotStyleValue.ToString();

			FloodingType = _vmLean.FloodingTypeValue.ToString();
		}
	}
}
