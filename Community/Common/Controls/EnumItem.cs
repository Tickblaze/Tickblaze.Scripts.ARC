using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class EnumItem : ReactiveObject
{
	[Reactive]
	[AllowNull]
	private string _name;

	[Reactive]
	[AllowNull]
	private string _displayName;
}
