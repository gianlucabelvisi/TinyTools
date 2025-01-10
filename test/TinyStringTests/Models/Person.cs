using TinyString;

// Make sure your test project references TinyString and imports this namespace.

namespace TinyStringTests.Models;

[PrintStyle(
    printStyle: PrintStyle.SingleLine,
    propertySeparator: " | ",
    keyValueSeparator: " = ",
    emoji: "🐱"
)]
[NamingFormat(NamingFormat.SnakeCase)]
[Rounding(1)]
public class Person
{
    public required string FirstName { get; set; }

    [NamingFormat(NamingFormat.CamelCase)]
    public required string LastName { get; set; }

    public double Height { get; set; }

    [Rounding(3)]
    public float Weight { get; set; }
}
