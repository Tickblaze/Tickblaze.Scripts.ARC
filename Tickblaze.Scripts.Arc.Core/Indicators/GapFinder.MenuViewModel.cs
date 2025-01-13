using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Core;

public partial class GapFinder
{
	public sealed class MenuViewModel : ReactiveObject
	{
		public MenuViewModel(GapFinder gapFinder)
		{
			_gapFinder = gapFinder;

			MenuHeader = _gapFinder.SettingsHeader;

			ShowFreshGaps = _gapFinder.ShowFreshGaps;
			ShowTestedGaps = _gapFinder.ShowTestedGaps;
			ShowBrokenGaps = _gapFinder.ShowBrokenGaps;
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
				_gapFinder.SettingsHeader = value;

				this.RaiseAndSetIfChanged(ref field, value);
			}
		}
	}
}
