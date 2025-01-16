using System;

namespace TinyString;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PropertyFormatAttribute : Attribute
{
    public PropertyFormatAttribute(string format)
    {
        Format = format;
    }

    /// <summary>
    /// Format string for this property.  
    /// Use `{k}` for the property name and `{v}` for the value.
    /// </summary>
    public string Format { get; set; }
}
