using TinyString;

namespace TinyStringTests.Models;

[PrintStyle(
    PrintStyle.MultiLine,
    printClassName: true)
]
public class Car
{
    public required string Brand { get; set; }
    public required double Price { get; set; }
}
