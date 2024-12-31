// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
namespace TinyOptional;

public static class OptionalExtensions
{
    /// <summary>
    /// Returns an Optional containing the first element of <paramref name="source"/>
    /// if any exists, otherwise an empty Optional.
    /// Enumerates only once.
    /// </summary>
    public static Optional<T> FirstIfExists<T>(this IEnumerable<T>? source)
    {
        switch (source)
        {
            case null:
            case ICollection<T> { Count: 0 }:
                return Optional<T>.Empty();
            // If it's a known ICollection<T> with no items, bail out quickly:
            default:
            {
                // Otherwise, check the first element
                using var e = source.GetEnumerator();
                return e.MoveNext()
                    ? Optional<T>.Of(e.Current)
                    : Optional<T>.Empty();
            }
        }
    }

    /// <summary>
    /// Returns an Optional containing the last element of <paramref name="source"/>
    /// if any exists, otherwise an empty Optional.
    /// Enumerates only once.
    /// </summary>
    public static Optional<T> LastIfExists<T>(this IEnumerable<T>? source)
    {
        switch (source)
        {
            case null:
                return Optional<T>.Empty();
            // If it's an IList<T>, we can directly index the last item
            case IList<T> list:
                return list.Count == 0
                    ? Optional<T>.Empty()
                    : Optional<T>.Of(list[^1]);
            // Or, if it's a collection with no items, return empty
            case ICollection<T> { Count: 0 }:
                return Optional<T>.Empty();
        }

        // Fallback: fully enumerate to find the last item
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
            return Optional<T>.Empty();

        var last = e.Current;
        while (e.MoveNext())
        {
            last = e.Current;
        }
        return Optional<T>.Of(last);
    }

    /// <summary>
    /// Returns an Optional containing the first element in <paramref name="source"/>
    /// matching <paramref name="predicate"/>, or an empty Optional if none match.
    /// Enumerates only once.
    /// </summary>
    public static Optional<T> FirstIfExists<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
        if (source == null)
            return Optional<T>.Empty();

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var item in source)
        {
            if (predicate(item))
                return Optional<T>.Of(item);
        }
        return Optional<T>.Empty();
    }

    /// <summary>
    /// Returns an Optional containing the last element in <paramref name="source"/>
    /// matching <paramref name="predicate"/>, or an empty Optional if none match.
    /// Enumerates only once.
    /// </summary>
    public static Optional<T> LastIfExists<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
        if (source == null)
            return Optional<T>.Empty();

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var found = false;
        T last = default!;

        foreach (var item in source)
        {
            if (predicate(item))
            {
                found = true;
                last = item;
            }
        }
        return found ? Optional<T>.Of(last) : Optional<T>.Empty();
    }

    /// <summary>
    /// Returns an Optional containing the string if <paramref name="str"/> is
    /// neither null nor empty, otherwise an empty Optional.
    /// Enumerates only once (string length check).
    /// </summary>
    public static Optional<string> IfAny(this string? str)
    {
        return string.IsNullOrEmpty(str)
            ? Optional<string>.Empty()
            : Optional<string>.Of(str);
    }
}
