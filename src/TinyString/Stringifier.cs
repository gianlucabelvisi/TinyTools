using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyString;

public static class Stringifier
{
    public static string? Stringify(this object? obj)
    {
        if (obj is null) return null;

        var objType = obj.GetType();

        // Get the attribute (if present) or create a default one
        var classAttr = objType.GetCustomAttribute<StringifyAttribute>() ?? new StringifyAttribute();

        var sb = new StringBuilder();

        // Class name / emoji
        if (classAttr.Emoji.IsNotEmpty())
        {
            sb.Append(classAttr.Emoji);
        }

        if (classAttr.PrintClassName)
        {
            sb.Append(sb.Length == 0 ? "" : " ");
            sb.Append(objType.Name);
        }

        // If we printed an emoji or class name, append line or ClassNameSeparator
        if (sb.Length > 0)
        {
            if (classAttr.PrintStyle == PrintStyle.MultiLine)
                sb.AppendLine();
            else
                sb.Append(classAttr.ClassNameSeparator);
        }

        // Gather public readable instance properties
        var props = objType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Where(p => p.CanRead)
            .Where(p => p.GetCustomAttribute<StringifyIgnoreAttribute>() == null);

        var firstProp = true;

        foreach (var prop in props)
        {
            var rawValue = prop.GetValue(obj);
            var propName = ConvertName(prop.Name, classAttr.NamingFormat);

            // Check for property-level attribute
            var propAttr = prop.GetCustomAttribute<StringifyPropertyAttribute>();

            // Determine property-specific settings
            var propFormat = propAttr?.Format ?? classAttr.Format;
            var propCollectionSeparator = propAttr?.CollectionSeparator ?? classAttr.CollectionSeparator;
            var propDecimals = propAttr?.Decimals ?? classAttr.Decimals;

            var propValue = ConvertValue(rawValue, propDecimals, propCollectionSeparator);

            var formattedProp = propFormat
                .Replace("{k}", propName)
                .Replace("{v}", propValue);

            if (classAttr.PrintStyle == PrintStyle.SingleLine)
            {
                if (!firstProp)
                {
                    sb.Append(classAttr.PropertySeparator);
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
            NamingFormat.KebabCase => name.ToKebabCase(),
            NamingFormat.HumanCase => name.ToHumanCase(),
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
            case IEnumerable enumerable when value is not string:
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
            case float f:
                return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", f);
            case double d:
                return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", d);
            case decimal m:
                return string.Format(CultureInfo.InvariantCulture, $"{{0:F{decimals}}}", m);
            default:
                // Check if it's a SmartEnum by looking for a Name property
                var type = value.GetType();
                var nameProperty = type.GetProperty("Name");

                if (nameProperty != null &&
                    nameProperty.PropertyType == typeof(string) &&
                    IsSmartEnumType(type))
                {
                    var name = nameProperty.GetValue(value) as string;
                    return name ?? value.ToString() ?? string.Empty;
                }

                // For other class types, call ToString()
                return value.ToString() ?? string.Empty;
        }
    }

    private static bool IsSmartEnumType(Type type)
    {
        // Check if the type is likely a SmartEnum by examining its inheritance hierarchy
        // This is a heuristic approach since we don't have direct access to the SmartEnum type

        // Check if the type has a base type that contains "SmartEnum" in its name
        var baseType = type.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            if (baseType.Name.Contains("SmartEnum"))
                return true;

            // Also check if it implements interfaces with SmartEnum in the name
            foreach (var interfaceType in baseType.GetInterfaces())
            {
                if (interfaceType.Name.Contains("SmartEnum"))
                    return true;
            }

            baseType = baseType.BaseType;
        }

        // Check if the type directly implements interfaces with SmartEnum in the name
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (interfaceType.Name.Contains("SmartEnum"))
                return true;
        }

        return false;
    }
}
