using FluentAssertions;
using TinyString;
using TinyStringTests.Models;

namespace TinyStringTests;

[TestFixture]
public class StringifyTests
{
    [Test]
    public void Animal_ShouldStringify_AsMultiLine()
    {
        // Arrange
        var animal = new Animal
        {
            Name = "Mittens",
            Species = Species.Cat,
            Weight = 4.567,
            Age = 5
        };

        // Act
        var result = animal.Stringify();

        // Assert
        result.Should().Be(
            "Animal: Name => Mittens, Species => Cat, Weight => 4.57, 5yrs, IsRare: False"
        );
    }

    [Test]
    public void Zoo_WithAnimals_ShouldStringify_AsSingleLine()
    {
        // Arrange
        var zoo = new Zoo
        {
            Title = "Wonderful Zoo",
            EntrancePrice = 15,
            Animals = [
                new Animal
                {
                    Name = "Mittens",
                    Species = Species.Cat,
                    Weight = 4.5,
                    IsRare = false,
                    Age = 5
                },
                new Animal
                {
                    Name = "Tony",
                    Species = Species.Tiger,
                    Weight = 120.3,
                    IsRare = true,
                    Age = 6
                },
                new Animal
                {
                    Name = "Dumbo",
                    Species = Species.Elephant,
                    Weight = 500.1,
                    IsRare = false,
                    Age = 10
                }
            ]
        };

        // Act
        var result = zoo.Stringify();

        // Assert
        result.Should().Be(
            @"🦁🦓🦍
Title: Wonderful Zoo
Animals: 
|_ Animal: Name => Mittens, Species => Cat, Weight => 4.50, 5yrs, IsRare: False
|_ Animal: Name => Tony, Species => Tiger, Weight => 120.30, 6yrs, IsRare: True
|_ Animal: Name => Dumbo, Species => Elephant, Weight => 500.10, 10yrs, IsRare: False
EntrancePrice: 15"
        );
    }

}
