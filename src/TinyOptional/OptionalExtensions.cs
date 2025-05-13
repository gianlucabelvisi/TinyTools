namespace TinyOptional;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

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
    /// Returns an optional containing the element at the specified index, if any
    /// </summary>
    /// <param name="source"></param>
    /// <param name="index"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Optional<T> ElementAtIfExists<T>(this IEnumerable<T>? source, int index)
    {
        if (source == null || index < 0)
        {
            return Optional<T>.Empty();
        }

        if (source is IList<T> list)
        {
            return index < list.Count ? Optional<T>.Of(list[index]) : Optional<T>.Empty();
        }

        using var enumerator = source.GetEnumerator();
        for (var i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext())
            {
                return Optional<T>.Empty(); // Index out of range
            }
            if (i == index)
            {
                return Optional<T>.Of(enumerator.Current);
            }
        }

        return Optional<T>.Empty(); // Should not reach here, but added for completeness
    }

    /// <summary>
    /// Provides an Optional containing the result of applying an accumulator function over a
    /// sequence, or an empty Optional
    /// if the sequence is empty.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="seed"></param>
    /// <param name="func"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TAccumulate"></typeparam>
    /// <returns></returns>
    public static Optional<TAccumulate> AggregateIfExists<TSource, TAccumulate>(
        this IEnumerable<TSource>? source, TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate>? func)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (source == null)
        {
            return Optional<TAccumulate>.Empty();
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return Optional<TAccumulate>.Empty();
        }

        var accumulate = seed;
        do
        {
            accumulate = func(accumulate, enumerator.Current);
        } while (enumerator.MoveNext());

        return Optional<TAccumulate>.Of(accumulate);
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
