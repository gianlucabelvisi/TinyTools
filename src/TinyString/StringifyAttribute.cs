namespace TinyString;

/// <summary>
/// An attribute to control how classes are stringified via the .Stringify() extension method.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class StringifyAttribute : Attribute
{
    /// <summary>
    /// Determines whether properties are printed on one line
    /// or multiple lines. Default = <see cref="PrintStyle.SingleLine"/>.
    /// </summary>
    public PrintStyle PrintStyle { get; set; } = PrintStyle.SingleLine;

    /// <summary>
    /// If <c>true</c>, prints the class name (or an emoji, if set) at the start
    /// before printing any properties. Default = <c>true</c>.
    /// </summary>
    public bool PrintClassName { get; set; } = true;

    /// <summary>
    /// If set, prints this emoji at the start instead of the class name.
    /// If <see cref="PrintClassName"/> is also <c>true</c> but <c>Emoji</c> is specified,
    /// the emoji takes precedence. Default = <c>""</c>.
    /// </summary>
    public string Emoji { get; set; } = "";

    /// <summary>
    /// A separator inserted immediately after the class name or emoji when
    /// <see cref="PrintStyle"/> is SingleLine.
    /// Default = <c>". "</c>.
    /// </summary>
    public string ClassNameSeparator { get; set; } = ". ";

    /// <summary>
    /// Used to separate properties when <see cref="PrintStyle"/> is SingleLine.
    /// Default = <c>", "</c>.
    /// </summary>
    public string PropertySeparator { get; set; } = ", ";

    /// <summary>
    /// Used to separate collection items (e.g. elements in a List).
    /// Default = <c>"; "</c>.
    /// </summary>
    public string CollectionSeparator { get; set; } = "; ";

    /// <summary>
    /// Number of decimal places to use when printing floating-point properties.
    /// Default = <c>5</c>.
    /// </summary>
    public int Decimals { get; set; } = 5;

    /// <summary>
    /// Specifies how property names are converted: PascalCase, CamelCase, SnakeCase, etc.
    /// Default = <see cref="NamingFormat.PascalCase"/>.
    /// </summary>
    public NamingFormat NamingFormat { get; set; } = NamingFormat.PascalCase;

    /// <summary>
    /// A format string controlling how each property is displayed.
    /// <c>{k}</c> is replaced by the property name; <c>{v}</c> is replaced by its value.
    /// Default = <c>"{k}: {v}"</c>.
    /// </summary>
    public string PropertyFormat { get; set; } = "{k}: {v}";
}
