using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Core;

public partial class FairValueGaps
{
	public partial class SettingsViewModel : ReactiveObject
	{
		public SettingsViewModel(FairValueGaps fairValueGaps)
		{
			_fairValueGaps = fairValueGaps;
		}

		private readonly FairValueGaps _fairValueGaps;

		public bool ShowFreshGaps
		{
			get;
			set
			{
				_fairValueGaps.ShowFreshGaps = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowTestedGaps
		{
			get;
			set
			{
				_fairValueGaps.ShowTestedGaps = field;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowBrokenGaps
		{
			get;
			set
			{
				_fairValueGaps.ShowBrokenGaps = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		[DisallowNull]
		public string? SettingsHeader
		{
			get;
			private set
			{
				_fairValueGaps.SettingsHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public void Initialize()
		{
			ShowFreshGaps = _fairValueGaps.ShowFreshGaps;
			ShowTestedGaps = _fairValueGaps.ShowTestedGaps;
			ShowBrokenGaps = _fairValueGaps.ShowBrokenGaps;
			SettingsHeader = _fairValueGaps.SettingsHeader;
		}
	}
}