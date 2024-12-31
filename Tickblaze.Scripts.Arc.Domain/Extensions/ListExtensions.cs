using Tickblaze.Scripts.Arc.Domain;

namespace Tickblaze.Scripts.Arc;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class ListExtensions
{
	public static IReadOnlyList<TDestination> Map<TSource, TDestination>(
		this IReadOnlyList<TSource> sourceList, Func<TSource, TDestination> selector)
	{
		return new ListTransform<TSource, TDestination>(sourceList, selector);
	}
}
