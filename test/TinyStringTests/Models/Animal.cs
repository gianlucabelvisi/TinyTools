using TinyString;

namespace TinyStringTests.Models;

[Stringify(
    PrintStyle = PrintStyle.SingleLine,
    PrintClassName = true,
    Format = "{k} => {v}",
    Decimals = 2
)]
public class Animal
{
    public required string Name { get; set; }

    public required Species Species { get; set; }

    public required double Weight { get; set; }

    [StringifyProperty(format:"{v}yrs")]
    public int Age { get; set; }

    [StringifyProperty(format: "{k}: {v}")]
    public bool IsRare { get; set; }
}
