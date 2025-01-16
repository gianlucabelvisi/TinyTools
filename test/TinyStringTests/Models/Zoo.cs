using TinyString;

namespace TinyStringTests.Models;

[Stringify(
    PrintStyle           = PrintStyle.SingleLine,
    PropertyFormat       = "{k} -> {v}",
    CollectionSeparator  = " | "
)]
public class Zoo
{
    public string Title { get; set; } = "Wonderful Zoo";

    // A list of Animals (which themselves have a [Stringify] attribute)
    public List<Animal> Animals { get; set; } = new()
    {
        new Animal
        {
            Name = "Mittens",
            Species = Species.Cat,
            Weight = 4.5,
            IsRare = false,
            Age = 3
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
        },
    };

    public double EntrancePrice { get; set; } = 15.0;
}
