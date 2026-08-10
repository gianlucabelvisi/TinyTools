namespace TinyOptional;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

public static class OptionalExtensions
{
    /// <summary>
    /// Returns an Optional containing the first element of <paramref name="source"/>
    /// if any exists, otherwise an empty Optional.
    /// </summary>
    public static Optional<T> FirstIfExists<T>(this IEnumerable<T>? source)
    {
        switch (source)
        {
            case null:
            case ICollection<T> { Count: 0 }:
                return Optional<T>.Empty();
            default:
            {
                using var e = source.GetEnumerator();
                return e.MoveNext()
                    ? Optional<T>.OfNullable(e.Current)
                    : Optional<T>.Empty();
            }
        }
    }

    /// <summary>
    /// Returns an Optional containing the first element in <paramref name="source"/>
    /// matching <paramref name="predicate"/>, or an empty Optional if none match.
    /// </summary>
    public static Optional<T> FirstIfExists<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
        if (source == null) return Optional<T>.Empty();
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        foreach (var item in source)
        {
            if (predicate(item)) return Optional<T>.OfNullable(item);
        }
        return Optional<T>.Empty();
    }

    /// <summary>
    /// Returns an Optional containing the last element of <paramref name="source"/>
    /// if any exists, otherwise an empty Optional.
    /// </summary>
    public static Optional<T> LastIfExists<T>(this IEnumerable<T>? source)
    {
        switch (source)
        {
            case null:
                return Optional<T>.Empty();
            case IList<T> list:
                return list.Count == 0 ? Optional<T>.Empty() : Optional<T>.OfNullable(list[^1]);
            case ICollection<T> { Count: 0 }:
                return Optional<T>.Empty();
        }

        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return Optional<T>.Empty();

        var last = e.Current;
        while (e.MoveNext()) last = e.Current;
        return Optional<T>.OfNullable(last);
    }

    /// <summary>
    /// Returns an Optional containing the last element in <paramref name="source"/>
    /// matching <paramref name="predicate"/>, or an empty Optional if none match.
    /// </summary>
    public static Optional<T> LastIfExists<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
        if (source == null) return Optional<T>.Empty();
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

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
        return found ? Optional<T>.OfNullable(last) : Optional<T>.Empty();
    }

    /// <summary>
    /// Returns an Optional containing the only element of <paramref name="source"/>,
    /// or empty if the source is null, empty, or contains more than one element.
    /// </summary>
    public static Optional<T> SingleIfExists<T>(this IEnumerable<T>? source)
    {
        if (source == null) return Optional<T>.Empty();
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return Optional<T>.Empty();
        var result = e.Current;
        return e.MoveNext() ? Optional<T>.Empty() : Optional<T>.OfNullable(result);
    }

    /// <summary>
    /// Returns an Optional containing the only element in <paramref name="source"/>
    /// matching <paramref name="predicate"/>, or empty if none or more than one match.
    /// </summary>
    public static Optional<T> SingleIfExists<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
        if (source == null) return Optional<T>.Empty();
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        T found = default!;
        var count = 0;
        foreach (var item in source)
        {
            if (!predicate(item)) continue;
            if (++count > 1) return Optional<T>.Empty();
            found = item;
        }
        return count == 1 ? Optional<T>.OfNullable(found) : Optional<T>.Empty();
    }

    /// <summary>
    /// Returns an Optional containing the element at the specified index, or empty
    /// if the source is null, the index is negative, or the index is out of range.
    /// </summary>
    public static Optional<T> ElementAtIfExists<T>(this IEnumerable<T>? source, int index)
    {
        if (source == null || index < 0) return Optional<T>.Empty();

        if (source is IList<T> list)
            return index < list.Count ? Optional<T>.OfNullable(list[index]) : Optional<T>.Empty();

        using var enumerator = source.GetEnumerator();
        for (var i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext()) return Optional<T>.Empty();
            if (i == index) return Optional<T>.OfNullable(enumerator.Current);
        }
        return Optional<T>.Empty();
    }

    /// <summary>
    /// Applies an accumulator function over a sequence and returns the result as an Optional,
    /// or empty if the source is null or empty.
    /// </summary>
    public static Optional<TAccumulate> AggregateIfExists<TSource, TAccumulate>(
        this IEnumerable<TSource>? source, TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate>? func)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        if (source == null) return Optional<TAccumulate>.Empty();

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) return Optional<TAccumulate>.Empty();

        var accumulate = seed;
        do
        {
            accumulate = func(accumulate, enumerator.Current);
        } while (enumerator.MoveNext());

        return Optional<TAccumulate>.OfNullable(accumulate);
    }

    /// <summary>
    /// Returns an Optional containing the string if it is neither null nor empty,
    /// otherwise an empty Optional.
    /// </summary>
    public static Optional<string> IfAny(this string? str)
    {
        return string.IsNullOrEmpty(str)
            ? Optional<string>.Empty()
            : Optional<string>.Of(str);
    }

    /// <summary>
    /// Wrap a nullable reference value into an Optional (empty if null).
    /// A concise alias for <see cref="Optional{T}.OfNullable"/>.
    /// </summary>
    public static Optional<T> ToOptional<T>(this T? value) where T : class
        => Optional<T>.OfNullable(value);

    /// <summary>
    /// Wrap a nullable value type into an Optional (empty if the <see cref="Nullable{T}"/> has no value).
    /// </summary>
    public static Optional<T> ToOptional<T>(this T? value) where T : struct
        => value.HasValue ? Optional<T>.Of(value.Value) : Optional<T>.Empty();

    /// <summary>
    /// Convert an Optional of a value type back to a <see cref="Nullable{T}"/>.
    /// </summary>
    public static T? ToNullable<T>(this Optional<T> optional) where T : struct
        => optional.IsPresent() ? optional.Get() : null;

    /// <summary>
    /// Flatten a nested Optional, collapsing <c>Optional&lt;Optional&lt;T&gt;&gt;</c> into <c>Optional&lt;T&gt;</c>.
    /// </summary>
    public static Optional<T> Flatten<T>(this Optional<Optional<T>> optional)
        => optional.IsPresent() ? optional.Get() : Optional<T>.Empty();

    /// <summary>
    /// Project each present value from a sequence of Optionals, discarding empties.
    /// </summary>
    public static IEnumerable<T> Values<T>(this IEnumerable<Optional<T>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        foreach (var opt in source)
        {
            if (opt is not null && opt.IsPresent()) yield return opt.Get();
        }
    }

    /// <summary>
    /// Returns an Optional containing the minimum element of <paramref name="source"/>,
    /// or empty if the source is null or empty.
    /// </summary>
    public static Optional<T> MinIfExists<T>(this IEnumerable<T>? source)
    {
        if (source == null) return Optional<T>.Empty();
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return Optional<T>.Empty();
        var comparer = Comparer<T>.Default;
        var min = e.Current;
        while (e.MoveNext())
            if (comparer.Compare(e.Current, min) < 0) min = e.Current;
        return Optional<T>.OfNullable(min);
    }

    /// <summary>
    /// Returns an Optional containing the maximum element of <paramref name="source"/>,
    /// or empty if the source is null or empty.
    /// </summary>
    public static Optional<T> MaxIfExists<T>(this IEnumerable<T>? source)
    {
        if (source == null) return Optional<T>.Empty();
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return Optional<T>.Empty();
        var comparer = Comparer<T>.Default;
        var max = e.Current;
        while (e.MoveNext())
            if (comparer.Compare(e.Current, max) > 0) max = e.Current;
        return Optional<T>.OfNullable(max);
    }
}
