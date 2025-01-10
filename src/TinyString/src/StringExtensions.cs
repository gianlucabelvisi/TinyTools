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
}
