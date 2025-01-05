//using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tickblaze.Scripts.Arc;

public partial class GapFinder
{
	public partial class SettingsViewModel //: ReactiveObject
	{
		public SettingsViewModel(GapFinder gapFinder)
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

				//this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowTestedGaps
		{
			get;
			set
			{
				_gapFinder.ShowTestedGaps = field;

				//this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public bool ShowBrokenGaps
		{
			get;
			set
			{
				_gapFinder.ShowBrokenGaps = value;

				//this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		[DisallowNull]
		public string? SettingsHeader
		{
			get;
			private set
			{
				_gapFinder.SettingsHeader = value;

				//this.RaiseAndSetIfChanged(ref field, value);
			}
		}

		public void Initialize()
		{
			ShowFreshGaps = _gapFinder.ShowFreshGaps;
			ShowTestedGaps = _gapFinder.ShowTestedGaps;
			ShowBrokenGaps = _gapFinder.ShowBrokenGaps;
			SettingsHeader = _gapFinder.SettingsHeader;
		}
	}
}
