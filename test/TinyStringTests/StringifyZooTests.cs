using FluentAssertions;
using TinyString;

namespace TinyStringTests;

[TestFixture]
public class StringifyZooTests
{
    [Stringify(
        Emoji                = "🦁🦓🦍",
        ClassNameSeparator   = "...",
        PrintStyle           = PrintStyle.MultiLine,
        PropertyFormat       = "{k} {v}",
        CollectionSeparator  = "\n|_ ",
        Decimals = 0
    )]
    public class Zoo
    {
        public required string Title { get; set; }
        public List<Animal> Animals { get; set; } = [];

        [StringifyProperty(format: "{k}: ${v})")]

        public double EntrancePrice { get; set; }
    }

    [Stringify(
        PrintStyle = PrintStyle.SingleLine,
        PrintClassName = false,
        PropertySeparator = " ",
        PropertyFormat = "{k} {v}",
        Decimals = 2
    )]
    public class Animal
    {
        [StringifyProperty(format:"{v}")]
        public required string Name { get; set; }

        [StringifyProperty(format:"({v})")]
        public required Species Species { get; set; }

        [StringifyProperty(format:"{v}kg")]
        public required double Weight { get; set; }

        [StringifyProperty(format:"{v}yrs")]
        public int Age { get; set; }

        [StringifyProperty(format: "(rare: {v})")]
        public bool IsRare { get; set; }
    }

    public enum Species
    {
        Cat,
        Tiger,
        Elephant
    }


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

        var str = animal.Stringify();

        str.Should().Be(
            "Mittens (Cat) 4.57kg 5yrs (rare: False)"
        );
    }

    [Test]
    public void Zoo_WithAnimals_ShouldStringify_MultiLine_Emoji_NoClassName()
    {
        var zoo = new Zoo
        {
            Title = "Wonderful Zoo",
            EntrancePrice = 14.99,
            Animals =
            [
                new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, IsRare = false, Age = 5 },
                new Animal { Name = "Tony", Species = Species.Tiger, Weight = 120.3, IsRare = true, Age = 6 },
                new Animal { Name = "Dumbo", Species = Species.Elephant, Weight = 500.1, IsRare = false, Age = 10 }
            ]
        };

        var str = zoo.Stringify();

        str.Should().Be(
@"🦁🦓🦍 Zoo
Title Wonderful Zoo
Animals 
|_ Mittens (Cat) 4.50kg 5yrs (rare: False)
|_ Tony (Tiger) 120.30kg 6yrs (rare: True)
|_ Dumbo (Elephant) 500.10kg 10yrs (rare: False)
EntrancePrice: $15)"
        );
    }

    [Test]
    public void DefaultClass_ShouldUse_DotSeparator()
    {
        var simpleClass = new SimpleClass { Name = "Test", Value = 42 };
        var result = simpleClass.Stringify();
        
        result.Should().Be("SimpleClass. Name: Test, Value: 42");
    }

    [Stringify]
    public class SimpleClass
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
