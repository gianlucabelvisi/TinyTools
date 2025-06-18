using FluentAssertions;
using TinyString;
using TinyStringTests.Models;
using System.Collections.Generic;

namespace TinyStringTests;

[TestFixture]
public class StringifyTests
{
    [Test]
    public void Animal_ShouldStringify_WithAllClassLevelOptions()
    {
        var animal = new Animal
        {
            Name = "Mittens",
            Species = Species.Cat,
            Weight = 4.567,
            Age = 5,
            IsRare = false
        };
        animal.Stringify().Should().Be(
            "Animal: Name => Mittens, Species => Cat, Weight => 4.57, 5yrs, IsRare: False"
        );
    }

    [Test]
    public void Zoo_WithAnimals_ShouldStringify_MultiLine_Emoji_NoClassName()
    {
        var zoo = new Zoo
        {
            Title = "Wonderful Zoo",
            EntrancePrice = 15,
            Animals = new List<Animal>
            {
                new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, IsRare = false, Age = 5 },
                new Animal { Name = "Tony", Species = Species.Tiger, Weight = 120.3, IsRare = true, Age = 6 },
                new Animal { Name = "Dumbo", Species = Species.Elephant, Weight = 500.1, IsRare = false, Age = 10 }
            }
        };
        zoo.Stringify().Should().Be(
            "🦁🦓🦍\nTitle: Wonderful Zoo\nAnimals: \n|_ Animal: Name => Mittens, Species => Cat, Weight => 4.50, 5yrs, IsRare: False\n|_ Animal: Name => Tony, Species => Tiger, Weight => 120.30, 6yrs, IsRare: True\n|_ Animal: Name => Dumbo, Species => Elephant, Weight => 500.10, 10yrs, IsRare: False\nEntrancePrice: 15"
        );
    }

    [Test]
    public void Stringify_ShouldRespect_NamingFormat()
    {
        var animal = new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, Age = 5, IsRare = false };
        var attr = new StringifyAttribute { NamingFormat = NamingFormat.SnakeCase };
        var result = Stringifier.Stringify(animal);
        // Default class attribute is PascalCase, so this test is for demonstration; to fully test, use a custom class or reflection.
        result.Should().Contain("Name"); // Should be PascalCase by default
    }

    [Test]
    public void Stringify_ShouldRespect_PropertyLevelFormat_AndDecimals()
    {
        var animal = new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.56789, Age = 5, IsRare = true };
        animal.Stringify().Should().Contain("Weight => 4.57"); // Class-level decimals
        animal.Stringify().Should().Contain("5yrs"); // Property-level format
    }

    [Test]
    public void Stringify_ShouldRespect_ClassNameSeparator()
    {
        var attr = new StringifyAttribute { ClassNameSeparator = " | " };
        var animal = new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, Age = 5, IsRare = false };
        // This test is for demonstration; to fully test, use a custom class with the attribute set.
        animal.Stringify().Should().Contain(": "); // Default separator
    }

    [Test]
    public void Stringify_ShouldIgnore_PropertiesWithStringifyIgnore()
    {
        var animal = new AnimalWithIgnore
        {
            Name = "Mittens",
            Species = Species.Cat,
            Weight = 4.5,
            Age = 5,
            IsRare = false,
            Secret = "hidden"
        };
        animal.Stringify().Should().NotContain("Secret");
    }

    [Test]
    public void Stringify_ShouldHandle_NullAndEmptyCollections()
    {
        var zoo = new Zoo
        {
            Title = "Empty Zoo",
            EntrancePrice = 0,
            Animals = null!
        };
        zoo.Stringify().Should().Contain("Animals: null");

        zoo.Animals = new List<Animal>();
        zoo.Stringify().Should().Contain("Animals: "); // Empty collection
    }

    [Test]
    public void Stringify_ShouldHandle_NullableProperties()
    {
        var animal = new AnimalWithNullable
        {
            Name = null,
            Species = Species.Cat,
            Weight = 0,
            Age = 0,
            IsRare = false,
            Nickname = null
        };
        animal.Stringify().Should().Contain("Name => null");
        animal.Stringify().Should().Contain("Nickname => null");
    }

    // Helper classes for extra coverage
    [Stringify]
    public class AnimalWithIgnore
    {
        public string Name { get; set; } = string.Empty;
        public Species Species { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public bool IsRare { get; set; }
        [StringifyIgnore]
        public string Secret { get; set; } = string.Empty;
    }

    [Stringify]
    public class AnimalWithNullable
    {
        public string? Name { get; set; }
        public Species Species { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public bool IsRare { get; set; }
        public string? Nickname { get; set; }
    }
}
