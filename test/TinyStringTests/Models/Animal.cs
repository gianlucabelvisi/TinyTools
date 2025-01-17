using TinyString;

namespace TinyStringTests.Models;

[Stringify(
    PrintStyle = PrintStyle.SingleLine,
    PrintClassName = true,
    PropertyFormat = "{k} => {v}",
    Decimals = 2
)]
public class Animal
{
    public required string Name { get; set; }

    // Now using an enum instead of a string
    public required Species Species { get; set; }

    public required double Weight { get; set; }

    [PropertyFormat("{v}yrs")]
    public int Age { get; set; }

    [PropertyFormat("{k}: {v}")]
    public bool IsRare { get; set; }
}
