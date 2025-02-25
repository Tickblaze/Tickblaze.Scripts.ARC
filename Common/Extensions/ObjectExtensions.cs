namespace Tickblaze.Community;

public static class ObjectExtensions
{
	public static string ToStringOrEmpty(this object? @object)
	{
		var @string = @object?.ToString();

        return @string ?? string.Empty;
	}
}
