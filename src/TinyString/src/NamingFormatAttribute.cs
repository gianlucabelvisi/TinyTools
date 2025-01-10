namespace TinyString;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class NamingFormatAttribute(NamingFormat namingFormat) : Attribute
{
    public NamingFormat NamingFormat { get; } = namingFormat;
}

public enum NamingFormat
{
    CamelCase,
    PascalCase,
    SnakeCase
}
