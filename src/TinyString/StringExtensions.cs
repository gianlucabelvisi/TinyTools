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

}
