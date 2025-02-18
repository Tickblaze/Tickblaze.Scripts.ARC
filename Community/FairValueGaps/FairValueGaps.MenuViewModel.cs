using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class FairValueGaps
{
	public sealed class MenuViewModel : ReactiveObject
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
			}
		}

		public bool ShowTestedGaps
		{
			get;
			set
			{
				_fairValueGaps.ShowTestedGaps = value;

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
			MenuHeader = _fairValueGaps.MenuHeader;

			ShowFreshGaps = _fairValueGaps.ShowFreshGaps;
			ShowTestedGaps = _fairValueGaps.ShowTestedGaps;
			ShowBrokenGaps = _fairValueGaps.ShowBrokenGaps;
		}
	}
}