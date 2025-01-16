using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyString;

public static class Stringifier
{
    public static string Stringify(this object? obj)
    {
        if (obj is null) return string.Empty;

        var objType = obj.GetType();

        // Get the attribute (if present)
        var attr = objType.GetCustomAttribute<StringifyAttribute>();

        // Fallback if attribute is missing
        if (attr is null)
        {
            return DefaultStringify(obj);
        }

        // Extract config
        var printStyle          = attr.PrintStyle;
        var printClassName      = attr.PrintClassName;
        var propertySeparator   = attr.PropertySeparator;
        var collectionSeparator = attr.CollectionSeparator;
        var emoji               = attr.Emoji ?? "";
        var decimals            = attr.Decimals;
        var namingFormat        = attr.NamingFormat;
        var classPropertyFormat = attr.PropertyFormat;
        var classNameSeparator  = attr.ClassNameSeparator; // <-- New

        var sb = new StringBuilder();

        // Class name / emoji
        if (!string.IsNullOrEmpty(emoji))
        {
            sb.Append(emoji);
        }
        else if (printClassName)
        {
            sb.Append(objType.Name);
        }

        // If we printed an emoji or class name, append line or ClassNameSeparator
        if (!string.IsNullOrEmpty(emoji) || printClassName)
        {
            if (printStyle == PrintStyle.MultiLine)
            {
                sb.AppendLine();
            }
            else
            {
                // Here is the change:
                sb.Append(classNameSeparator);
            }
        }

        // Gather public readable instance properties
        var props = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(p => p.GetIndexParameters().Length == 0)
                           .Where(p => p.CanRead);

        bool firstProp = true;

        foreach (var prop in props)
        {
            var rawValue  = prop.GetValue(obj);
            var propName  = ConvertName(prop.Name, namingFormat);
            var propValue = ConvertValue(rawValue, decimals, collectionSeparator, namingFormat);

            // Check for a property-level format override
            var propFormatAttr = prop.GetCustomAttribute<PropertyFormatAttribute>();
            var propFormat = propFormatAttr?.Format ?? classPropertyFormat;

            var formattedProp = propFormat
                .Replace("{k}", propName)
                .Replace("{v}", propValue);

            // SingleLine vs. MultiLine
            if (printStyle == PrintStyle.SingleLine)
            {
                if (!firstProp)
                {
                    sb.Append(propertySeparator);
                }
                sb.Append(formattedProp);
                firstProp = false;
            }
            else
            {
                sb.AppendLine(formattedProp);
            }
        }

        return sb.ToString().TrimEnd(); // optional: remove trailing newline/spaces
    }

    private static string DefaultStringify(object obj)
    {
        return obj.ToString() ?? string.Empty;
    }

    private static string ConvertName(string name, NamingFormat namingFormat)
    {
        return namingFormat switch
        {
            NamingFormat.CamelCase => name.ToCamelCase(),
            NamingFormat.SnakeCase => name.ToSnakeCase(),
            _ => name, // default PascalCase
        };
    }

    private static string ConvertValue(
        object? value,
        int decimals,
        string collectionSeparator,
        NamingFormat namingFormat)
    {
        if (value is null) return "null";

        if (value is string s)
            return s;

        if (value is IEnumerable enumerable && !(value is char))
        {
            var items = new List<string>();
            foreach (var item in enumerable)
            {
                items.Add(ConvertValue(item, decimals, collectionSeparator, namingFormat));
            }
            return string.Join(collectionSeparator, items);
        }

        var type = value.GetType();
        if (type == typeof(float) || type == typeof(double))
        {
            return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", value);
        }

        if (type.GetCustomAttribute<StringifyAttribute>() is not null)
        {
            return value.Stringify();
        }

        return value.ToString() ?? string.Empty;
    }
}
