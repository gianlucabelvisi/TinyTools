namespace TinyString;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StringifyPropertyAttribute : Attribute
{
    /// <summary>
    /// Creates a new StringifyPropertyAttribute with optional format.
    /// </summary>
    /// <param name="format">Optional format string for this property.</param>
    public StringifyPropertyAttribute(string? format = null)
    {
        Format = format;
    }

    /// <summary>
    /// Format string for this property.
    /// Use `{k}` for the property name and `{v}` for the value.
    /// Default = <c>null</c> (uses class-level format).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Used to separate collection items (e.g. elements in a List).
    /// Default = <c>null</c> (uses class-level separator).
    /// </summary>
    public string? CollectionSeparator { get; set; }

    /// <summary>
    /// Number of decimal places to use when printing floating-point properties.
    /// Default = <c>null</c> (uses class-level decimal places).
    /// </summary>
    public int? Decimals { get; set; }

    /// <summary>
    /// A separator inserted immediately after the class name or emoji.
    /// Default = <c>null</c> (uses class-level separator).
    /// </summary>
    public string? ClassNameSeparator { get; set; }
}
