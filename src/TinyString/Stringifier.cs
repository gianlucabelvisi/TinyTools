using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyString;

// Bundles the context that flows through the recursive rendering calls.
internal sealed record RenderContext(
    int Decimals,
    string CollectionSeparator,
    string NullToken,
    IReadOnlyDictionary<Type, Func<object, string>>? NestedRenderers);

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

    internal static string StringifyWithOptions<T>(T obj, StringifyOptions<T> opts)
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

            // Explicit ignore
            if (cfg?.Ignored == true) continue;

            // Only whitelist
            if (opts._only != null && !opts._only.Contains(prop.Name)) continue;

            var rawValue = prop.GetValue(obj);

            // Conditional display
            if (cfg?.ShowWhen != null && !cfg.ShowWhen(rawValue)) continue;

            var keyName  = ConvertName(cfg?.Label ?? prop.Name, opts._namingFormat);
            var showKey  = cfg?.ShowKey ?? true;
            var prefix   = cfg?.Prefix ?? "";
            var suffix   = cfg?.Suffix ?? "";
            var decimals = cfg?.Decimals ?? opts._decimals;
            var collSep  = cfg?.CollectionSeparator
                        ?? opts._collectionSeparator
                        ?? (opts._style == PrintStyle.MultiLine ? "\n|_ " : ", ");

            // Value conversion — custom formatter takes full control when set
            string convertedValue;
            var nullToken = cfg?.NullAs ?? opts._nullToken;

            if (cfg?.ValueFormatter != null)
            {
                convertedValue = cfg.ValueFormatter(rawValue);
            }
            else
            {
                // Apply Transform before anything else
                var effectiveValue = cfg?.Transform != null ? cfg.Transform(rawValue) : rawValue;

                // Highlight (conditional formatter)
                if (cfg?.HighlightPredicate != null && cfg.HighlightFormatter != null
                         && cfg.HighlightPredicate(effectiveValue))
                {
                    convertedValue = cfg.HighlightFormatter(effectiveValue);
                }
                // BoolAs
                else if (cfg?.BoolTrueLabel != null && effectiveValue is bool bVal)
                {
                    convertedValue = bVal ? cfg.BoolTrueLabel : cfg.BoolFalseLabel!;
                }
                // DateFormat
                else if (cfg?.DateFormat != null && effectiveValue is IFormattable dateVal
                         && IsDateLike(effectiveValue))
                {
                    convertedValue = dateVal.ToString(cfg.DateFormat, CultureInfo.InvariantCulture);
                }
                // NumberFormat
                else if (cfg?.NumberFormat != null && effectiveValue is IFormattable numVal
                         && effectiveValue is int or long or short or byte or uint or ulong
                            or ushort or sbyte or float or double or decimal)
                {
                    convertedValue = numVal.ToString(cfg.NumberFormat, CultureInfo.InvariantCulture);
                }
                else
                {
                    var ctx = new RenderContext(decimals, collSep, nullToken, opts._nestedRenderers);
                    convertedValue = ConvertValue(effectiveValue, ctx, cfg?.MaxItems);
                }
            }

            // Value replacements (match against the rendered string)
            if (cfg?.ValueReplacements != null
                && cfg.ValueReplacements.TryGetValue(convertedValue, out var replacement))
            {
                convertedValue = replacement;
            }

            // Truncate
            if (cfg?.TruncateLength is { } maxLen && convertedValue.Length > maxLen)
                convertedValue = convertedValue[..maxLen] + "…";

            var rendered = prefix + convertedValue + suffix;
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

    // Called internally (no obsolete warning) for legacy-attribute objects and nested fallback.
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
            var ctx       = new RenderContext(decimals, collSep, "null", null);
            var converted = ConvertValue(value, ctx);
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

    private static bool IsDateLike(object? value) => value is DateTime or DateTimeOffset
        || value?.GetType().Name is "DateOnly" or "TimeOnly";

    private static string ConvertName(string name, NamingFormat format) => format switch
    {
        NamingFormat.CamelCase  => name.ToCamelCase(),
        NamingFormat.SnakeCase  => name.ToSnakeCase(),
        NamingFormat.KebabCase  => name.ToKebabCase(),
        NamingFormat.HumanCase  => name.ToHumanCase(),
        _                       => name,
    };

    internal static string ConvertValue(object? value, RenderContext ctx, int? maxItems = null) =>
        value switch
        {
            null                    => ctx.NullToken,
            string s                => s,
            bool b                  => b.ToString(),
            Enum e                  => e.ToString(),
            int or long or short
                or byte or uint
                or ulong or ushort
                or sbyte            => value.ToString()!,
            IEnumerable enumerable  => ConvertCollection(enumerable, ctx, maxItems),
            float f                 => f.ToString($"F{ctx.Decimals}", CultureInfo.InvariantCulture),
            double d                => d.ToString($"F{ctx.Decimals}", CultureInfo.InvariantCulture),
            decimal m               => m.ToString($"F{ctx.Decimals}", CultureInfo.InvariantCulture),
            _                       => ConvertComplex(value, ctx.NestedRenderers),
        };

    private static string ConvertComplex(object value, IReadOnlyDictionary<Type, Func<object, string>>? nestedRenderers)
    {
        // 1. Registered ForNested renderer
        if (nestedRenderers != null && nestedRenderers.TryGetValue(value.GetType(), out var renderer))
            return renderer(value);

        // 2. SmartEnum heuristic
        var smartEnum = TrySmartEnum(value);
        if (smartEnum != null) return smartEnum;

        // 3. Overridden ToString() — if the type provides its own, respect it
        if (value.GetType().GetMethod("ToString", Type.EmptyTypes)?.DeclaringType != typeof(object))
            return value.ToString()!;

        // 4. Reflect over the object with legacy defaults
        return StringifyLegacy(value);
    }

    private static string ConvertCollection(IEnumerable enumerable, RenderContext ctx, int? maxItems)
    {
        var separator = ctx.CollectionSeparator;
        var allItems  = enumerable.Cast<object?>().ToList();

        // Apply MaxItems truncation
        int overflow = 0;
        if (maxItems.HasValue && allItems.Count > maxItems.Value)
        {
            overflow = allItems.Count - maxItems.Value;
            allItems = allItems.Take(maxItems.Value).ToList();
        }

        // Render each item (maxItems does not propagate into nested collections)
        var rendered = allItems
            .Select(i => ConvertValue(i, ctx))
            .ToList<string>();

        if (overflow > 0)
            rendered.Add($"... and {overflow} more");

        if (!separator.Contains('\n'))
            return string.Join(separator, rendered);

        // Multi-line: indent subsequent lines of each item so they align under the prefix.
        // e.g. separator "\n|_ " → prefix "|_ " → indent "   " (same width)
        var listPrefix = separator[(separator.LastIndexOf('\n') + 1)..];
        var indent     = new string(' ', listPrefix.Length);
        var indented   = rendered.Select(s => s.Replace("\n", "\n" + indent));

        return separator + string.Join(separator, indented);
    }

    private static string? TrySmartEnum(object value)
    {
        var type     = value.GetType();
        var nameProp = type.GetProperty("Name");
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
