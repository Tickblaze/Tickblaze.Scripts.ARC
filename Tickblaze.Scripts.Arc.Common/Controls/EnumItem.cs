using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Arc.Common;

public partial class EnumItem : ReactiveObject
{
	[Reactive]
	[AllowNull]
	private object _value;

	[Reactive]
	[AllowNull]
	private string _displayName;
}
