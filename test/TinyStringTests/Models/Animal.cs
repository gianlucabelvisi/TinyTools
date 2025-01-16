using TinyString;

namespace TinyStringTests.Models;

[Stringify(
    PrintClassName = false,
    PropertyFormat = "{k} => {v}"
)]
public class Animal
{
    public string Name       { get; set; } = "Mittens";

    // Now using an enum instead of a string
    public Species Species   { get; set; } = Species.Cat;

    public double Weight     { get; set; } = 4.5;
    public bool IsRare       { get; set; } = false;

    // This property uses a property-level format override:
    [PropertyFormat("[{k} -> {v}]")]
    public int Age           { get; set; } = 3;
}
