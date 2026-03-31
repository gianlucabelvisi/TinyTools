using System.Linq.Expressions;

namespace TinyString;

/// <summary>
/// Fluent builder for configuring how an object is stringified.
/// Obtain one via <see cref="Stringifier.Stringify{T}(T, Action{StringifyOptions{T}})"/>.
/// </summary>
public sealed class StringifyOptions<T>
{
    // ── Global settings (internal so Stringifier can read them directly) ────

    internal PrintStyle _style = PrintStyle.SingleLine;
    internal string? _header;
    internal bool _showHeader = true;
    internal string _separator = ", ";
    internal string? _collectionSeparator; // null = auto (", " single-line / "\n|_ " multi-line)
    internal int _decimals = 2;
    internal NamingFormat _namingFormat = NamingFormat.PascalCase;
    internal string _nullToken = "null";
    internal HashSet<string>? _only;

    // ── Per-property settings ──────────────────────────────────────────────

    internal readonly Dictionary<string, PropertyConfig> _properties = new();

    // ── Nested type renderers ──────────────────────────────────────────────

    internal readonly Dictionary<Type, Func<object, string>> _nestedRenderers = new();

    // ── Global option methods ──────────────────────────────────────────────

    /// <summary>Print each property on its own line.</summary>
    public StringifyOptions<T> MultiLine() { _style = PrintStyle.MultiLine; return this; }

    /// <summary>Print all properties on a single line (default).</summary>
    public StringifyOptions<T> SingleLine() { _style = PrintStyle.SingleLine; return this; }

    /// <summary>Override the header shown before the properties (replaces the class name).</summary>
    public StringifyOptions<T> Label(string label) { _header = label; return this; }

    /// <summary>Hide the header entirely.</summary>
    public StringifyOptions<T> NoLabel() { _showHeader = false; return this; }

    /// <summary>Separator between properties in single-line mode. Default: <c>", "</c>.</summary>
    public StringifyOptions<T> Separator(string separator) { _separator = separator; return this; }

    /// <summary>
    /// Override the default separator between collection items.
    /// By default: <c>", "</c> in single-line mode, <c>"\n|_ "</c> in multi-line mode.
    /// </summary>
    public StringifyOptions<T> CollectionSeparator(string separator) { _collectionSeparator = separator; return this; }

    /// <summary>Default number of decimal places for floating-point values. Default: <c>2</c>.</summary>
    public StringifyOptions<T> Decimals(int decimals) { _decimals = decimals; return this; }

    /// <summary>Naming format applied to all property keys. Default: <see cref="NamingFormat.PascalCase"/>.</summary>
    public StringifyOptions<T> Keys(NamingFormat format) { _namingFormat = format; return this; }

    /// <summary>String used when a property value is <c>null</c>. Default: <c>"null"</c>.</summary>
    public StringifyOptions<T> NullAs(string token) { _nullToken = token; return this; }

    /// <summary>
    /// Show only the specified properties; all others are excluded.
    /// This is the inverse of chaining multiple <c>.For(...).Ignore()</c> calls.
    /// </summary>
    public StringifyOptions<T> Only(params Expression<Func<T, object?>>[] selectors)
    {
        _only = new HashSet<string>(selectors.Select(ExtractNameBoxed));
        return this;
    }

    // ── Nested type configuration ──────────────────────────────────────────

    /// <summary>
    /// Configure how a specific nested type is rendered wherever it appears —
    /// as a direct property or inside a collection. The full fluent API is
    /// available for the inner type.
    /// </summary>
    public StringifyOptions<T> ForNested<TNested>(Action<StringifyOptions<TNested>> configure)
    {
        var nestedOpts = new StringifyOptions<TNested>();
        configure(nestedOpts);
        _nestedRenderers[typeof(TNested)] = obj => Stringifier.StringifyWithOptions((TNested)obj, nestedOpts);
        return this;
    }

    // ── Property configuration ─────────────────────────────────────────────

    /// <summary>
    /// Begin configuring a specific property. Chain further calls on the returned
    /// <see cref="PropertyBuilder{T, TProp}"/> and continue with the next
    /// <c>.For()</c> when done.
    /// </summary>
    public PropertyBuilder<T, TProp> For<TProp>(Expression<Func<T, TProp>> selector)
    {
        var name = ExtractName(selector);
        if (!_properties.TryGetValue(name, out var config))
        {
            config = new PropertyConfig();
            _properties[name] = config;
        }
        return new PropertyBuilder<T, TProp>(this, config);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    internal static string ExtractName<TProp>(Expression<Func<T, TProp>> selector) =>
        selector.Body is MemberExpression m
            ? m.Member.Name
            : throw new ArgumentException(
                "Selector must be a direct property access, e.g. x => x.Name.", nameof(selector));

    // Expression<Func<T, object?>> boxes value types into a Convert node — unwrap it.
    private static string ExtractNameBoxed(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;
        if (body is UnaryExpression { NodeType: System.Linq.Expressions.ExpressionType.Convert } u)
            body = u.Operand;
        return body is MemberExpression m
            ? m.Member.Name
            : throw new ArgumentException(
                "Selector must be a direct property access, e.g. x => x.Name.", nameof(selector));
    }
}

/// <summary>Per-property configuration, populated by <see cref="PropertyBuilder{T, TProp}"/>.</summary>
internal sealed class PropertyConfig
{
    public bool Ignored { get; set; }
    public string? Label { get; set; }
    public bool ShowKey { get; set; } = true;
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? CollectionSeparator { get; set; }
    public int? Decimals { get; set; }
    public int? MaxItems { get; set; }
    public Func<object?, bool>? ShowWhen { get; set; }
    public Func<object?, string>? ValueFormatter { get; set; }
}
