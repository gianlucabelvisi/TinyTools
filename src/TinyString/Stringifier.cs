using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyString;

public static class Stringifier
{
    // ── New fluent API ──────────────────────────────────────────────────────

    /// <summary>
    /// Converts an object to a human-readable string.
    /// Pass an optional <paramref name="configure"/> action to customise the output
    /// with the fluent builder; omit it for sensible defaults.
    /// </summary>
    public static string? Stringify<T>(this T obj, Action<StringifyOptions<T>>? configure = null)
    {
        if (obj is null) return null;

        // When called with no options, honour any legacy attributes so that
        // existing code keeps working without changes.
        if (configure is null && HasLegacyAttributes(typeof(T)))
            return StringifyLegacy(obj);

        var options = new StringifyOptions<T>();
        configure?.Invoke(options);
        return StringifyWithOptions(obj, options);
    }

    private static string StringifyWithOptions<T>(T obj, StringifyOptions<T> opts)
    {
        var type = typeof(T);
        var sb = new StringBuilder();

        // Header
        if (opts._showHeader)
        {
            sb.Append(opts._header ?? type.Name);
            if (opts._style == PrintStyle.MultiLine)
                sb.AppendLine();
            else
                sb.Append(". ");
        }

        var props = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead);

        var first = true;
        foreach (var prop in props)
        {
            opts._properties.TryGetValue(prop.Name, out var cfg);
            if (cfg?.Ignored == true) continue;

            var keyName  = ConvertName(cfg?.Label ?? prop.Name, opts._namingFormat);
            var showKey  = cfg?.ShowKey ?? true;
            var prefix   = cfg?.Prefix ?? "";
            var suffix   = cfg?.Suffix ?? "";
            var decimals = cfg?.Decimals ?? opts._decimals;
            var collSep  = cfg?.CollectionSeparator ?? opts._collectionSeparator;

            var value    = prop.GetValue(obj);
            var rendered = prefix + ConvertValue(value, decimals, collSep) + suffix;
            var line     = showKey ? $"{keyName}: {rendered}" : rendered;

            if (opts._style == PrintStyle.SingleLine)
            {
                if (!first) sb.Append(opts._separator);
                sb.Append(line);
                first = false;
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString().TrimEnd();
    }

    // ── Legacy attribute-based API (deprecated) ─────────────────────────────

    /// <summary>
    /// Converts an object to a string using the legacy attribute-based configuration
    /// (<c>[Stringify]</c>, <c>[StringifyProperty]</c>, <c>[StringifyIgnore]</c>).
    /// </summary>
    /// <remarks>
    /// This overload is kept for backwards compatibility. Migrate to
    /// <see cref="Stringify{T}(T, Action{StringifyOptions{T}})"/> with the fluent API.
    /// Attribute-based configuration will be removed in a future major version.
    /// </remarks>
    [Obsolete(
        "Attribute-based configuration is deprecated. " +
        "Use Stringify<T>(Action<StringifyOptions<T>>) with the fluent API instead. " +
        "Attribute-based configuration will be removed in a future major version.")]
    public static string? Stringify(this object? obj) =>
        obj is null ? null : StringifyLegacy(obj);

    // Called internally (no obsolete warning) for legacy-attribute objects and nested objects.
    internal static string StringifyLegacy(object obj)
    {
        var type      = obj.GetType();
        var classAttr = type.GetCustomAttribute<StringifyAttribute>() ?? new StringifyAttribute();
        var sb        = new StringBuilder();

        if (classAttr.Emoji.IsNotEmpty())
            sb.Append(classAttr.Emoji);

        if (classAttr.PrintClassName)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(type.Name);
        }

        if (sb.Length > 0)
        {
            if (classAttr.PrintStyle == PrintStyle.MultiLine)
                sb.AppendLine();
            else
                sb.Append(classAttr.ClassNameSeparator);
        }

        var props = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead)
            .Where(p => p.GetCustomAttribute<StringifyIgnoreAttribute>() == null);

        var first = true;
        foreach (var prop in props)
        {
            var propAttr  = prop.GetCustomAttribute<StringifyPropertyAttribute>();
            var propName  = ConvertName(prop.Name, classAttr.NamingFormat);
            var format    = propAttr?.Format ?? classAttr.PropertyFormat;
            var collSep   = propAttr?.CollectionSeparator ?? classAttr.CollectionSeparator;
            var decimals  = propAttr?.Decimals ?? classAttr.Decimals;

            var value     = prop.GetValue(obj);
            var converted = ConvertValue(value, decimals, collSep);
            var line      = format.Replace("{k}", propName).Replace("{v}", converted);

            if (classAttr.PrintStyle == PrintStyle.SingleLine)
            {
                if (!first) sb.Append(classAttr.PropertySeparator);
                sb.Append(line);
                first = false;
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString().TrimEnd();
    }

    // ── Shared helpers ──────────────────────────────────────────────────────

    private static bool HasLegacyAttributes(Type type)
    {
        if (type.GetCustomAttribute<StringifyAttribute>() != null) return true;
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetCustomAttribute<StringifyPropertyAttribute>() != null
                   || p.GetCustomAttribute<StringifyIgnoreAttribute>() != null);
    }

    private static string ConvertName(string name, NamingFormat format) => format switch
    {
        NamingFormat.CamelCase  => name.ToCamelCase(),
        NamingFormat.SnakeCase  => name.ToSnakeCase(),
        NamingFormat.KebabCase  => name.ToKebabCase(),
        NamingFormat.HumanCase  => name.ToHumanCase(),
        _                       => name,
    };

    internal static string ConvertValue(object? value, int decimals, string collectionSeparator) =>
        value switch
        {
            null                    => "null",
            string s                => s,
            bool b                  => b.ToString(),
            Enum e                  => e.ToString(),
            int or long or short
                or byte or uint
                or ulong or ushort
                or sbyte            => value.ToString()!,
            IEnumerable enumerable  => ConvertCollection(enumerable, decimals, collectionSeparator),
            float f                 => f.ToString($"F{decimals}", CultureInfo.InvariantCulture),
            double d                => d.ToString($"F{decimals}", CultureInfo.InvariantCulture),
            decimal m               => m.ToString($"F{decimals}", CultureInfo.InvariantCulture),
            _                       => TrySmartEnum(value) ?? StringifyLegacy(value),
        };

    private static string ConvertCollection(IEnumerable enumerable, int decimals, string separator)
    {
        var items  = enumerable.Cast<object?>().Select(i => ConvertValue(i, decimals, separator));
        var joined = string.Join(separator, items);
        // If the separator starts with a newline the caller wants it also before the first item.
        return separator.Contains('\n') ? separator + joined : joined;
    }

    private static string? TrySmartEnum(object value)
    {
        var type         = value.GetType();
        var nameProp     = type.GetProperty("Name");
        if (nameProp?.PropertyType != typeof(string)) return null;

        bool IsSmartEnum(Type t) =>
            t.GetInterfaces().Any(i => i.Name.Contains("SmartEnum"));

        var cursor = type;
        while (cursor != null && cursor != typeof(object))
        {
            if (cursor.Name.Contains("SmartEnum") || IsSmartEnum(cursor))
                return nameProp.GetValue(value) as string;
            cursor = cursor.BaseType;
        }
        return IsSmartEnum(type) ? nameProp.GetValue(value) as string : null;
    }
}
