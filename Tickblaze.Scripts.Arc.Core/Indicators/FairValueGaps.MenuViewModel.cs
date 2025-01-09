using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Core;

public partial class FairValueGaps
{
	public partial class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(FairValueGaps fairValueGaps)
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

				//this.RaisePropertyChanged(nameof(ShowFreshGapsMenuHeader));
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
		public string? MenuHeader
		{
			get;
			private set
			{
				_fairValueGaps.MenuHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public void Initialize()
		{
			ShowFreshGaps = _fairValueGaps.ShowFreshGaps;
			ShowTestedGaps = _fairValueGaps.ShowTestedGaps;
			ShowBrokenGaps = _fairValueGaps.ShowBrokenGaps;
			MenuHeader = _fairValueGaps.MenuHeader;
		}
	}
}