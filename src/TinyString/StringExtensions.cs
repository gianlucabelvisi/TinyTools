using System.Text.RegularExpressions;

namespace TinyString;

using System.Text;

public static class StringExtensions
{
    /// <summary>
    /// Naive CamelCase conversion: first letter lowercase, rest unchanged.
    /// (You can enhance with better rules for acronyms, underscores, etc.)
    /// </summary>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        if (str.Length == 1) return str.ToLower();
        return char.ToLower(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Simple SnakeCase conversion: insert underscores before uppercase letters, then lower everything.
    /// (Again, can be improved for various corner cases.)
    /// </summary>
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        var sb = new StringBuilder();
        foreach (char c in str)
        {
            if (char.IsUpper(c) && sb.Length > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLower(c));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    public static string ToKebabCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        var snakeCase = str.ToSnakeCase();
        return snakeCase.Replace('_', '-');
    }

    /// <summary>
    /// Converts a string to Human Case with spaces between words.
    /// </summary>
    public static string ToHumanCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        var sb = new StringBuilder();

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            // Add space before uppercase letters if not the first character
            // and the previous character is not already a space
            if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(str[i - 1]))
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Simple Slug conversion: remove non-alphanumeric characters and lowercase everything.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSlug(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]", "").ToLower();

    /// <summary>
    /// Remove newline characters from a string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string OneLine(this string? str) => str?.Replace("\n", " ").Replace("\r", " ") ?? "";

    /// <summary>
    /// Split in the capped part and the residual part.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static (string capped, string residual) CapLength(this string str, int length)
        => new
        (
            str.Length <= length ? str : str[..length],
            str.Length > length ? str[length..] : string.Empty
        );

    /// <summary>
    /// Check if a string is composed of only digits.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsDigitsOnly(this string str) => str.All(char.IsDigit);

    /// <summary>
    /// Remove all non-digit characters from a string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string KeepDigits(this string str)
        => string.IsNullOrEmpty(str) ? string.Empty : new string(str.Where(char.IsDigit).ToArray());

    /// <summary>
    /// Check if a string is null or empty.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// Check if a string is not null or empty.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNotEmpty(this string? str) => !string.IsNullOrEmpty(str);

    /// <summary>
    /// Join a sequence
    /// </summary>
    /// <param name="source"></param>
    /// <param name="separator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

    /// <summary>
    /// Truncate a string to <paramref name="maxLength"/> characters, appending
    /// <paramref name="ellipsis"/> when truncation occurs. The ellipsis is included in the
    /// resulting length so the output never exceeds <paramref name="maxLength"/>.
    /// </summary>
    public static string Truncate(this string? str, int maxLength, string ellipsis = "…")
    {
        if (string.IsNullOrEmpty(str) || maxLength < 0) return str ?? string.Empty;
        if (str!.Length <= maxLength) return str;
        if (maxLength <= ellipsis.Length) return str[..maxLength];
        return str[..(maxLength - ellipsis.Length)] + ellipsis;
    }

    /// <summary>
    /// Repeat a string <paramref name="count"/> times.
    /// </summary>
    public static string Repeat(this string? str, int count)
    {
        if (string.IsNullOrEmpty(str) || count <= 0) return string.Empty;
        return string.Concat(Enumerable.Repeat(str, count));
    }

    /// <summary>
    /// Convert a string to Title Case (first letter of each word upper-cased).
    /// </summary>
    public static string ToTitleCase(this string? str)
    {
        if (string.IsNullOrEmpty(str)) return str ?? string.Empty;
        var sb = new StringBuilder(str!.Length);
        var newWord = true;
        foreach (var c in str)
        {
            if (char.IsWhiteSpace(c))
            {
                newWord = true;
                sb.Append(c);
            }
            else
            {
                sb.Append(newWord ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                newWord = false;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Ensure the string starts with <paramref name="prefix"/>, prepending it if missing.
    /// </summary>
    public static string EnsurePrefix(this string? str, string prefix)
    {
        str ??= string.Empty;
        return str.StartsWith(prefix, StringComparison.Ordinal) ? str : prefix + str;
    }

    /// <summary>
    /// Ensure the string ends with <paramref name="suffix"/>, appending it if missing.
    /// </summary>
    public static string EnsureSuffix(this string? str, string suffix)
    {
        str ??= string.Empty;
        return str.EndsWith(suffix, StringComparison.Ordinal) ? str : str + suffix;
    }

    /// <summary>
    /// Return null if the string is null or empty, otherwise the string itself.
    /// Handy for null-coalescing defaults: <c>value.NullIfEmpty() ?? "fallback"</c>.
    /// </summary>
    public static string? NullIfEmpty(this string? str) => string.IsNullOrEmpty(str) ? null : str;

    /// <summary>
    /// Reverse the characters of a string.
    /// </summary>
    public static string Reverse(this string? str)
    {
        if (string.IsNullOrEmpty(str)) return str ?? string.Empty;
        var chars = str!.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}
