using FluentAssertions;
using NUnit.Framework;
using TinyString;
using TinyStringTests.Models;

namespace TinyStringTests;

[TestFixture]
public class StringifyTests
{
    [Test]
    public void Person_Stringify_ShouldUseEmojiAndSingleLine()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "DOE",
            Height = 185.56789,
            Weight = 70.1234f
        };

        // Act
        var actual = person.Stringify();

        // Example of what we expect:
        // "Type: 🐱 | first_name = John | lastName = DOE | height = 185.6 | weight = 70.123"
        // Explanation:
        //  - "🐱" replaced the class name
        //  - "first_name" is from [NamingFormat(NamingFormat.SnakeCase)] at the class level
        //  - "lastName" is from [NamingFormat(NamingFormat.CamelCase)] at the property level
        //  - "height" has 1 decimal (from [Rounding(1)] at the class level)
        //  - "weight" has 3 decimals (from [Rounding(3)] at the property level)
        //  - Single-line output with propertySeparator = " | " and keyValueSeparator = " = "

        var expected = "🐱 | first_name = John | lastName = DOE | height = 185.6 | weight = 70.123";

        // Assert with FluentAssertions
        actual.Should().Be(expected);
    }

    [Test]
    public void Car_Stringify_ShouldUseClassNameAndMultiLine()
    {
        // Arrange
        var car = new Car
        {
            Brand = "Tesla",
            Price = 54999.99
        };

        // Act
        string actual = car.Stringify();

        // Expected multi-line output:
        // Type: Car
        // Brand: Tesla
        // Price: 54999.99
        //
        // We'll represent it as a multi-line string with \n.
        // Note: If you're on Windows, you might get "\r\n" by default.
        // We'll normalize newlines to avoid OS-specific issues.
        string expected =
            "Car\n" +
            "Brand: Tesla\n" +
            "Price: 54999.99\n";

        // Assert with FluentAssertions
        NormalizeNewLines(actual).Should().Be(NormalizeNewLines(expected));
    }

    /// <summary>
    /// Helper to ensure consistent line endings (e.g.,
    /// converting "\r\n" to "\n") to avoid OS-specific mismatches.
    /// </summary>
    private string NormalizeNewLines(string input)
    {
        return input
            .Replace("\r\n", "\n") // Convert Windows line breaks to LF
            .TrimEnd(); // Remove trailing whitespace/newlines if desired
    }
}
