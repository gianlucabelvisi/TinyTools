using FluentAssertions;
using TinyString;
using TinyStringTests.Models;

namespace TinyStringTests;

[TestFixture]
public class StringifyTests
{
    [Test]
    public void Animal_WithDefaultValues_ShouldStringify_AsMultiLine()
    {
        // Arrange
        // Animal uses: [Stringify(PropertyFormat = "{k} => {v}")]
        // By default, PrintStyle is MultiLine (unless changed in the attribute).
        var animal = new Animal(); // "Mittens", Species.Cat, Weight=4.5, etc.

        // Act
        var result = animal.Stringify();

        // Assert
        // We expect something like:
        // Animal
        // Name => Mittens
        // Species => Cat
        // Weight => 4.50
        // IsRare => False
        // [Age -> 3]
        //
        // Notice: [Age -> 3] is overridden by [PropertyFormat("[{k} -> {v}]")].
        //
        // If your decimals or booleans differ in casing/format, adjust accordingly.
        result.Should().Be(
            @"Animal
Name => Mittens
Species => Cat
Weight => 4.50
IsRare => False
[Age -> 3]"
        );
    }

    [Test]
    public void Zoo_WithAnimals_ShouldStringify_AsSingleLine()
    {
        // Arrange
        // Zoo uses: [Stringify(PrintStyle = PrintStyle.SingleLine, PropertyFormat = "{k} -> {v}", CollectionSeparator = " | ")]
        //   Title = "Wonderful Zoo"
        //   Animals = List<Animal> { new Animal(...), ... }
        //   EntrancePrice = 15.0
        var zoo = new Zoo();

        // Act
        var result = zoo.Stringify();

        // Assert
        // Because PrintStyle is SingleLine, we expect all properties in one line,
        // separated by the Zoo's propertySeparator (default ", "),
        // and each Animal separated by " | " in the collection.
        //
        // Each Animal is itself multi-line by default, but within a collection
        // it prints as a single string.
        // The "[Age -> x]" is due to the property-level override in Animal.
        //
        // Adjust spacing/punctuation if your actual code differs.
        result.Should().Be(
            "Zoo: Title -> Wonderful Zoo, " +
            "Animals -> Name => Mittens, Species => Cat, Weight => 4.50, IsRare => False, [Age -> 3] | " +
            "Name => Tony, Species => Tiger, Weight => 120.30, IsRare => True, [Age -> 6] | " +
            "Name => Dumbo, Species => Elephant, Weight => 500.10, IsRare => False, [Age -> 10], " +
            "EntrancePrice -> 15.00"
        );
    }

    [Test]
    public void Animal_WithCustomValues_ShouldStringify_AsExpected()
    {
        // Arrange
        var animal = new Animal
        {
            Name    = "Oscar",
            Species = Species.Tiger,
            Weight  = 65.789,
            IsRare  = true,
            Age     = 8
        };

        // Act
        var result = animal.Stringify();

        // Assert
        // By default, multi-line. Decimals are set to 2 places (unless changed).
        result.Should().Be(
            @"Name => Oscar, Species => Tiger, Weight => 65.79, IsRare => True, [Age -> 8]"
        );
    }
}
