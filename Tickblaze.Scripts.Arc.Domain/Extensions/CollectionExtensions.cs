namespace Tickblaze.Scripts.Arc;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Performs a binary search on a sorted list to find the index of the specified item.
    /// </summary>
    /// <typeparam name="TItem">The type of elements in the list.</typeparam>
    /// <param name="items">The sorted list of items.</param>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item if found; otherwise, a negative index indicating where the item would be inserted.</returns>
    /// <remarks>
    /// The implementation is a refactored copy of <see href="https://github.com/dotnet/runtime/blob/eee9fd32dbf29d4df263c940b394a8af47bd5cf5/src/libraries/System.Private.CoreLib/src/System/SpanHelpers.BinarySearch.cs#L13">this</see>.
    /// </remarks>
    public static int BinarySearch<TItem>(this IReadOnlyList<TItem> items, TItem item)
    {
        var lowerIndex = 0;
        var higherIndex = items.Count - 1;

        while (lowerIndex <= higherIndex)
        {
            var medianIndex = lowerIndex + (higherIndex - lowerIndex) / 2;

            var compareResult = Comparer<TItem>.Default.Compare(item, items[medianIndex]);

            if (compareResult is 0)
            {
                return medianIndex;
            }
            else if (compareResult > 0)
            {
                lowerIndex = medianIndex + 1;
            }
            else
            {
                higherIndex = medianIndex - 1;
            }
        }

        return ~lowerIndex;
    }
}
