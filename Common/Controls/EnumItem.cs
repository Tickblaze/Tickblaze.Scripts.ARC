using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Community;

public partial class EnumItem : ReactiveObject
{	
	[field: AllowNull]
	public string? Name
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	[field: AllowNull]
	public string DisplayName
	{
		get;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
}
