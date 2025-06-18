using TinyString;

namespace TinyStringTests.Models;

[Stringify(
    PrintClassName       = false,
    Emoji                = "🦁🦓🦍",
    PrintStyle           = PrintStyle.MultiLine,
    Format       = "{k}: {v}",
    PropertySeparator    = "; ",
    CollectionSeparator  = "\n|_ ",
    Decimals = 0
)]
public class Zoo
{
    public required string Title { get; set; }

    public List<Animal> Animals { get; set; } = [];

    public double EntrancePrice { get; set; }
}
