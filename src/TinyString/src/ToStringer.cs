using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyString;

public static class ToStringer
{
    public static string Stringify(this object? obj)
    {
        if (obj == null) return string.Empty;

        var objType = obj.GetType();

        // Attributes
        var classNamingAttr   = objType.GetCustomAttribute<NamingFormatAttribute>();
        var classRoundingAttr = objType.GetCustomAttribute<RoundingAttribute>();
        var printStyleAttr    = objType.GetCustomAttribute<PrintStyleAttribute>();

        // PrintStyle parameters
        var printStyle = printStyleAttr?.PrintStyle ?? PrintStyle.MultiLine;
        var printClassName= printStyleAttr?.PrintClassName     ?? true;
        var propertySeparator = printStyleAttr?.PropertySeparator  ?? ", ";
        var keyValueSeparator = printStyleAttr?.KeyValueSeparator  ?? ": ";
        var emoji = printStyleAttr?.Emoji ?? "";

        // Start building the output
        var sb = new StringBuilder();

        // Check whether to prefix with an emoji, the class name, or nothing
        if (!string.IsNullOrEmpty(emoji))
            sb.Append(emoji);
        else if (printClassName)
            sb.Append(objType.Name);

        if (!string.IsNullOrEmpty(emoji) || printClassName)
        {
            if (printStyle == PrintStyle.MultiLine)
                sb.AppendLine();
            else
                sb.Append(propertySeparator);
        }

        // Iterate each public property
        var props = objType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.GetIndexParameters().Length == 0) // Skip indexers or properties we can't read
            .Where(prop => prop.CanRead); // Skip properties we can't read

        var firstProp = true;

        foreach (var prop in props)
        {
            // Property value
            var value = prop.GetValue(obj);

            // Determine naming format (property-level overrides class-level)
            var propNamingAttr = prop.GetCustomAttribute<NamingFormatAttribute>();
            var namingFormat = propNamingAttr?.NamingFormat
                ?? classNamingAttr?.NamingFormat
                ?? NamingFormat.PascalCase;

            // Property-level attributs
            var propRoundingAttr = prop.GetCustomAttribute<RoundingAttribute>();
            var decimals = propRoundingAttr?.Decimals
                ?? classRoundingAttr?.Decimals
                ?? 2;

            var propertyName = ConvertName(prop.Name, namingFormat);

            var propertyValue = ConvertValue(value, decimals);

            // Output format depends on SingleLine or MultiLine
            if (printStyle == PrintStyle.SingleLine)
            {
                if (!firstProp)
                {
                    sb.Append(propertySeparator);
                }
                sb.Append($"{propertyName}{keyValueSeparator}{propertyValue}");
                firstProp = false;
            }
            else
            {
                sb.AppendLine($"{propertyName}{keyValueSeparator}{propertyValue}");
            }
        }

        return sb.ToString();
    }

    private static string ConvertName(string name, NamingFormat namingFormat)
    {
        return namingFormat switch
        {
            NamingFormat.CamelCase => name.ToCamelCase(),
            NamingFormat.SnakeCase => name.ToSnakeCase(),
            _ => name, // default (PascalCase)
        };
    }

    private static string ConvertValue(object? value, int decimals)
    {
        if (value == null) return "null";

        var type = value.GetType();
        if (type == typeof(float) || type == typeof(double))
        {
            return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", value);
        }
        return value.ToString() ?? string.Empty;
    }

}
