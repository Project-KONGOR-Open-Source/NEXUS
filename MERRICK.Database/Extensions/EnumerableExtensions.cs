namespace MERRICK.Database.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Returns TRUE if the collection is empty. Otherwise, returns FALSE.
    /// </summary>
    public static bool None<TSource>(this IEnumerable<TSource> collection)
        => !collection.Any();

    /// <summary>
    ///     Returns TRUE if no element in the collection satisfies the condition specified by a predicate. Otherwise, returns FALSE.
    /// </summary>
    public static bool None<TSource>(this IEnumerable<TSource> collection, Func<TSource, bool> predicate)
        => !collection.Any(predicate);

    /// <summary>
    ///     Returns TRUE if the collection is empty. Otherwise, returns FALSE.
    /// </summary>
    public static async Task<bool> NoneAsync<TSource>(this IQueryable<TSource> collection, CancellationToken cancellationToken = default)
        => !await collection.AnyAsync(cancellationToken);

    /// <summary>
    ///     Returns TRUE if no element in the collection satisfies the condition specified by a predicate. Otherwise, returns FALSE.
    /// </summary>
    public static async Task<bool> NoneAsync<TSource>(this IQueryable<TSource> collection, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        => !await collection.AnyAsync(predicate, cancellationToken);

    /// <summary>
    ///     Returns a new collection composed of the input collection's elements in a random order.
    /// </summary>
    public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> collection)
        => collection.OrderBy(element => Random.Shared.Next());

    /// <summary>
    ///     Returns a random element from the collection.
    /// </summary>
    public static TSource RandomElement<TSource>(this IEnumerable<TSource> collection)
        => collection.Shuffle().First();
}
