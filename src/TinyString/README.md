# TinyString

![Logo](https://raw.githubusercontent.com/gianlucabelvisi/TinyTools/main/src/TinyString/logo_icon.png)

**TinyString** is a lightweight .NET library that turns any object into a readable string — with zero boilerplate for simple cases and a clean fluent API when you need more control.

---

## Installation

```bash
dotnet add package TinyString
```

---

## Quick Start

Call `.Stringify()` on any object and get a sensible result with no configuration:

```csharp
using TinyString;

var book = new Book { Title = "1984", Author = "George Orwell", Pages = 328 };
book.Stringify();
// → "Book. Title: 1984, Author: George Orwell, Pages: 328"
```

Public properties are printed in declaration order. The class name is used as the header, separated by `". "`. Floats default to 2 decimal places.

---

## Fluent API

Pass a configuration action to `Stringify()` to customise the output at the call site — no attributes needed on your classes:

```csharp
var zoo = new Zoo
{
    Name = "Woodland Zoo",
    EntrancePrice = 14.99,
    Animals = [ new Animal { Name = "Mittens", Species = "Cat", Weight = 4.5 }, ... ]
};

zoo.Stringify(o => o
    .MultiLine()
    .Label("🦁 Zoo")
    .For(x => x.EntrancePrice).Prefix("$").Decimals(0)
    .For(x => x.Animals).Separator("\n|_ "));
```

```
🦁 Zoo
Name: Woodland Zoo
EntrancePrice: $15
Animals:
|_ Animal. Name: Mittens, Species: Cat, Weight: 4.50
|_ Animal. Name: Tony, Species: Tiger, Weight: 120.30
```

---

## Global Options

| Method | Description | Default |
|---|---|---|
| `.MultiLine()` | One property per line | — |
| `.SingleLine()` | All on one line | ✓ |
| `.Label("text")` | Override the class name header | Class name |
| `.NoLabel()` | Hide the header | — |
| `.Separator("…")` | Between properties (single-line) | `", "` |
| `.CollectionSeparator("…")` | Between collection items (all properties) | `"; "` |
| `.Decimals(n)` | Decimal places for floats | `2` |
| `.Keys(NamingFormat.X)` | Key naming style | `PascalCase` |

---

## Per-Property Options

Start a property chain with `.For(x => x.Property)`:

| Method | Description |
|---|---|
| `.Ignore()` | Exclude this property entirely |
| `.As("label")` | Override the display name of the key |
| `.NoKey()` | Show only the value, no key |
| `.Prefix("…")` | Prepend text before the value |
| `.Suffix("…")` | Append text after the value |
| `.Separator("…")` | Collection separator for this property |
| `.Decimals(n)` | Decimal places for this property |

Continuing with `.For()` at the end of any property chain moves to the next property, so the whole configuration reads as one fluent expression:

```csharp
order.Stringify(o => o
    .NoLabel()
    .Separator(" | ")
    .For(x => x.Price).Prefix("$").Decimals(2)
    .For(x => x.Discount).Suffix("%").NoKey()
    .For(x => x.InternalId).Ignore());
```

---

## Naming Formats

| Format | Example |
|---|---|
| `PascalCase` (default) | `FirstName` |
| `CamelCase` | `firstName` |
| `SnakeCase` | `first_name` |
| `KebabCase` | `first-name` |
| `HumanCase` | `First Name` |

```csharp
item.Stringify(o => o.Keys(NamingFormat.HumanCase));
// → "Item. First Name: Ada, Is Active: True"
```

---

## Nested Objects & Collections

Nested objects are stringified automatically using their own defaults (or their own `.Stringify()` call, if any):

```csharp
var order = new Order
{
    Product = new Product { Name = "Book", Price = 12.99 },
    Qty = 2
};

order.Stringify();
// → "Order. Product: Product. Name: Book, Price: 12.99, Qty: 2"
```

Collections are joined with the active separator:

```csharp
order.Stringify(o => o.For(x => x.Tags).Separator(" · "));
// → "Order. Tags: sci-fi · classic · dystopian, ..."
```

---

## Full Example: Animal Compact Format

```csharp
animal.Stringify(o => o
    .NoLabel()
    .Separator(" ")
    .For(x => x.Name).NoKey()
    .For(x => x.Species).Prefix("(").Suffix(")").NoKey()
    .For(x => x.Weight).NoKey().Suffix("kg").Decimals(2)
    .For(x => x.Age).NoKey().Suffix("yrs")
    .For(x => x.IsRare).Ignore());

// → "Mittens (Cat) 4.50kg 5yrs"
```

---

## Legacy Attribute API (Deprecated)

Previous versions used attributes to configure stringification. These still work but will produce deprecation warnings and will be removed in a future major version.

```csharp
// ⚠️ Deprecated
[Stringify(PrintStyle = PrintStyle.MultiLine, Emoji = "🦁")]
public class Zoo
{
    [StringifyProperty(format: "${v}")]
    public double EntrancePrice { get; set; }

    [StringifyIgnore]
    public string InternalCode { get; set; } = "";
}
```

Migrate by moving the configuration to the `.Stringify(o => o…)` call site instead.

---

## License

TinyString is licensed under the MIT License.
