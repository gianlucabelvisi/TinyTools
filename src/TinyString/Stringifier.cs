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
            return obj.ToString() ?? string.Empty;
        }

        // Object configuration
        var printStyle                = attr.PrintStyle;
        var printClassName       = attr.PrintClassName;
        var emoji              = attr.Emoji;
        var propertySeparator   = attr.PropertySeparator;
        var collectionSeparator = attr.CollectionSeparator;
        var classNameSeparator  = attr.ClassNameSeparator;
        var decimals              = attr.Decimals;
        var namingFormat              = attr.NamingFormat;
        var classPropertyFormat = attr.PropertyFormat;

        var sb = new StringBuilder();

        // Class name / emoji
        if (emoji.IsNotEmpty())
        {
            sb.Append(emoji);
        }

        if (printClassName)
        {
            sb.Append(sb.Length == 0 ? "" : " ");
            sb.Append(objType.Name);
        }

        // If we printed an emoji or class name, append line or ClassNameSeparator
        if (sb.Length > 0)
        {
            if (printStyle == PrintStyle.MultiLine)
                sb.AppendLine();
            else
                sb.Append(classNameSeparator);
        }

        // Gather public readable instance properties
        var props = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(p => p.GetIndexParameters().Length == 0)
                           .Where(p => p.CanRead);

        var firstProp = true;

        foreach (var prop in props)
        {
            var rawValue  = prop.GetValue(obj);
            var propName  = ConvertName(prop.Name, namingFormat);
            var propValue = ConvertValue(rawValue, decimals, collectionSeparator);

            // Check for a property-level format override
            var propFormatAttr = prop.GetCustomAttribute<PropertyFormatAttribute>();
            var propFormat = propFormatAttr?.Format ?? classPropertyFormat;

            var formattedProp = propFormat
                .Replace("{k}", propName)
                .Replace("{v}", propValue);

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

        return sb.ToString().TrimEnd();
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

    private static string ConvertValue(object? value, int decimals, string collectionSeparator)
    {
        switch (value)
        {
            case null:
                return "null";
            case string s:
                return s;
            case IEnumerable enumerable when value is not char:
            {
                var items = new List<string>();

                foreach (var item in enumerable)
                {
                    items.Add(ConvertValue(item, decimals, collectionSeparator));
                }
                var itemStr = items.Join(collectionSeparator);

                if (collectionSeparator.Contains("\n"))
                    itemStr = collectionSeparator + itemStr;

                return itemStr;
            }
        }

        var type = value.GetType();
        if (type == typeof(float) || type == typeof(double))
        {
            return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", value);
        }

        return value.Stringify();
    }
}
