using System.Linq.Expressions;

namespace TinyString;

/// <summary>
/// Fluent builder for configuring how a single property is rendered.
/// Obtained via <see cref="StringifyOptions{T}.For{TProp}"/>.
/// </summary>
public sealed class PropertyBuilder<T, TProp>
{
    private readonly StringifyOptions<T> _parent;
    private readonly PropertyConfig _config;

    internal PropertyBuilder(StringifyOptions<T> parent, PropertyConfig config)
    {
        _parent = parent;
        _config = config;
    }

    /// <summary>Exclude this property from the output.</summary>
    public PropertyBuilder<T, TProp> Ignore() { _config.Ignored = true; return this; }

    /// <summary>Override the display name of this property's key.</summary>
    public PropertyBuilder<T, TProp> As(string name) { _config.Label = name; return this; }

    /// <summary>Render only the value — no key prefix.</summary>
    public PropertyBuilder<T, TProp> NoKey() { _config.ShowKey = false; return this; }

    /// <summary>Prepend text immediately before this property's value.</summary>
    public PropertyBuilder<T, TProp> Prefix(string prefix) { _config.Prefix = prefix; return this; }

    /// <summary>Append text immediately after this property's value.</summary>
    public PropertyBuilder<T, TProp> Suffix(string suffix) { _config.Suffix = suffix; return this; }

    /// <summary>
    /// Override the separator used between items when this property is a collection.
    /// When the separator starts with <c>\n</c>, it is also prepended before the first item
    /// and multi-line items are indented to align under the prefix.
    /// </summary>
    public PropertyBuilder<T, TProp> Separator(string separator) { _config.CollectionSeparator = separator; return this; }

    /// <summary>Override the number of decimal places for this property's value.</summary>
    public PropertyBuilder<T, TProp> Decimals(int decimals) { _config.Decimals = decimals; return this; }

    /// <summary>
    /// Limit the number of items shown when this property is a collection.
    /// Any remaining items are replaced with <c>"... and N more"</c>.
    /// </summary>
    public PropertyBuilder<T, TProp> MaxItems(int max) { _config.MaxItems = max; return this; }

    /// <summary>
    /// Show this property only when the predicate returns <c>true</c> for its value.
    /// Useful for suppressing default/empty/uninteresting values.
    /// </summary>
    public PropertyBuilder<T, TProp> When(Func<TProp, bool> predicate)
    {
        _config.ShowWhen = obj => obj is TProp v && predicate(v);
        return this;
    }

    /// <summary>
    /// Replace the entire value conversion with a custom formatter.
    /// Takes precedence over <see cref="Prefix"/>, <see cref="Suffix"/>,
    /// <see cref="Decimals"/>, and <see cref="Separator"/>.
    /// </summary>
    public PropertyBuilder<T, TProp> ValueFormat(Func<TProp, string> formatter)
    {
        _config.ValueFormatter = obj => obj is TProp v ? formatter(v) : string.Empty;
        return this;
    }

    /// <summary>
    /// Replace specific values with custom strings. Works with any type.
    /// Values not found in the dictionary are rendered normally.
    /// </summary>
    public PropertyBuilder<T, TProp> ReplaceValues(Dictionary<TProp, string> replacements)
    {
        _config.ValueReplacements = replacements.ToDictionary(
            kv => kv.Key?.ToString() ?? "null",
            kv => kv.Value);
        return this;
    }

    /// <summary>
    /// Format date/time values using a .NET format string (e.g. <c>"yyyy-MM-dd"</c>).
    /// Applies to <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, and <see cref="DateOnly"/>.
    /// </summary>
    public PropertyBuilder<T, TProp> DateFormat(string format)
    {
        _config.DateFormat = format;
        return this;
    }

    /// <summary>
    /// Display <c>true</c> / <c>false</c> as custom labels (e.g. <c>"Yes"</c> / <c>"No"</c> or <c>"✅"</c> / <c>"❌"</c>).
    /// </summary>
    public PropertyBuilder<T, TProp> BoolAs(string trueLabel, string falseLabel)
    {
        _config.BoolTrueLabel = trueLabel;
        _config.BoolFalseLabel = falseLabel;
        return this;
    }

    /// <summary>
    /// Cap the rendered string length. Values longer than <paramref name="maxLength"/>
    /// are cut and end with <c>"…"</c>.
    /// </summary>
    public PropertyBuilder<T, TProp> Truncate(int maxLength)
    {
        _config.TruncateLength = maxLength;
        return this;
    }

    /// <summary>
    /// Apply a transformation to the value before rendering.
    /// The result is then converted to string using the normal rules.
    /// </summary>
    public PropertyBuilder<T, TProp> Transform(Func<TProp, object?> transform)
    {
        _config.Transform = obj => obj is TProp v ? transform(v) : obj;
        return this;
    }

    /// <summary>Render this property's value as UPPER CASE.</summary>
    public PropertyBuilder<T, TProp> UpperCase()
    {
        _config.Transform = obj => obj?.ToString()?.ToUpperInvariant();
        return this;
    }

    /// <summary>Render this property's value as lower case.</summary>
    public PropertyBuilder<T, TProp> LowerCase()
    {
        _config.Transform = obj => obj?.ToString()?.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Override the null token for this property only.
    /// Takes precedence over the global <see cref="StringifyOptions{T}.NullAs"/>.
    /// </summary>
    public PropertyBuilder<T, TProp> NullAs(string token)
    {
        _config.NullAs = token;
        return this;
    }

    /// <summary>
    /// Format numeric values using a .NET format string (e.g. <c>"N0"</c> for thousands separators,
    /// <c>"C"</c> for currency, <c>"P1"</c> for percentage).
    /// </summary>
    public PropertyBuilder<T, TProp> NumberFormat(string format)
    {
        _config.NumberFormat = format;
        return this;
    }

    /// <summary>
    /// Apply a custom formatter only when the predicate is true.
    /// Values that don't match the predicate are rendered normally.
    /// </summary>
    public PropertyBuilder<T, TProp> Highlight(Func<TProp, bool> predicate, Func<TProp, string> formatter)
    {
        _config.HighlightPredicate = obj => obj is TProp v && predicate(v);
        _config.HighlightFormatter = obj => obj is TProp v ? formatter(v) : string.Empty;
        return this;
    }

    /// <summary>
    /// Continue configuring the next property. Equivalent to calling
    /// <c>.For()</c> on the parent <see cref="StringifyOptions{T}"/>.
    /// </summary>
    public PropertyBuilder<T, TNext> For<TNext>(Expression<Func<T, TNext>> selector)
        => _parent.For(selector);
}
