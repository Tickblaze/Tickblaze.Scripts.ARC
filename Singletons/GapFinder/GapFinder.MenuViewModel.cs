using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class GapFinder
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(GapFinder gapFinder)
		{
			_gapFinder = gapFinder;
		}

		private readonly GapFinder _gapFinder;

		public bool ShowFreshGaps
		{
			get;
			set
			{
				_gapFinder.ShowFreshGaps = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowTestedGaps
		{
			get;
			set
			{
				_gapFinder.ShowTestedGaps = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowBrokenGaps
		{
			get;
			set
			{
				_gapFinder.ShowBrokenGaps = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		[DisallowNull]
		public string? MenuHeader
		{
			get;
			private set
			{
				_gapFinder.MenuHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public void Initialize()
		{
			MenuHeader = _gapFinder.MenuHeader;

			ShowFreshGaps = _gapFinder.ShowFreshGaps;
			ShowTestedGaps = _gapFinder.ShowTestedGaps;
			ShowBrokenGaps = _gapFinder.ShowBrokenGaps;
		}
	}
}
