// ReSharper disable UnusedMember.Global
namespace Optional;

public static class OptionalExtensions
{
    private static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable) {
        return enumerable == null || !enumerable.Any();
    }

    public static Optional<T> FirstIfExists<T>(this ICollection<T>? source)
    {
        return source.IsNullOrEmpty() ? Optional<T>.Empty() : Optional<T>.Of(source!.FirstOrDefault());
    }
    public static Optional<T> LastIfExists<T>(this ICollection<T>? source)
    {
        return source.IsNullOrEmpty() ? Optional<T>.Empty() : Optional<T>.Of(source!.LastOrDefault());
    }

    public static Optional<T> FirstIfExists<T>(this ICollection<T> source, Func<T, bool> predicate)
    {
        return source.IsNullOrEmpty() ? Optional<T>.Empty() : Optional<T>.Of(source.FirstOrDefault(predicate));
    }

    public static Optional<T> LastIfExists<T>(this ICollection<T> source, Func<T, bool> predicate)
    {
        return source.IsNullOrEmpty() ? Optional<T>.Empty() : Optional<T>.Of(source.LastOrDefault(predicate));
    }

    public static Optional<string> IfAny(this string? str)
    {
        return str.IsNullOrEmpty() ? Optional<string>.Empty() : Optional<string>.Of(str);
    }

}
