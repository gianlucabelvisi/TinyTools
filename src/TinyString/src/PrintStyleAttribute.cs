namespace TinyString;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PrintStyleAttribute(
    PrintStyle printStyle,
    bool printClassName = true,
    string propertySeparator = ", ",
    string keyValueSeparator = ": ",
    string? emoji = null)
    : Attribute
{
    public PrintStyle PrintStyle { get; } = printStyle;

    /// <summary>
    /// If true, "Type: {ClassName}" is printed (unless Emoji is provided).
    /// </summary>
    public bool PrintClassName { get; } = printClassName;

    /// <summary>
    /// The string that separates one property from another in SingleLine mode.
    /// </summary>
    public string PropertySeparator { get; } = propertySeparator;

    /// <summary>
    /// The string that separates the property name and its value.
    /// </summary>
    public string KeyValueSeparator { get; } = keyValueSeparator;

    /// <summary>
    /// If set, will be printed instead of the class name. (e.g. "🐱", "🚀")
    /// </summary>
    public string? Emoji { get; } = emoji;
}

public enum PrintStyle
{
    SingleLine,
    MultiLine
}
