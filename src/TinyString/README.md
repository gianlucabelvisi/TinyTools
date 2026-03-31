# TinyString

![Logo](https://raw.githubusercontent.com/gianlucabelvisi/TinyTools/main/src/TinyString/logo_icon.png)

Turn any .NET object into a readable string. Zero config for simple cases, a clean fluent API when you want more.

```bash
dotnet add package TinyString
```

---

## Zero config

Call `.Stringify()` on anything:

```csharp
using TinyString;

var book = new Book { Title = "1984", Author = "George Orwell", Pages = 328 };
book.Stringify();
// → "Book. Title: 1984, Author: George Orwell, Pages: 328"
```

All public properties, declaration order, class name as header, floats at 2 decimal places. No setup required.

---

## Fluent API

Pass a configuration action when you need more control. Everything lives at the call site — your classes stay clean:

```csharp
zoo.Stringify(o => o
    .MultiLine()
    .Label("🦁 Zoo")
    .For(x => x.EntrancePrice).Prefix("$").Decimals(0));
```

```
🦁 Zoo
Name: Woodland Zoo
EntrancePrice: $15
Animals:
|_ Animal. Name: Mittens, Species: Cat, Weight: 4.50
|_ Animal. Name: Tony, Species: Tiger, Weight: 120.30
```

In multi-line mode, collections are automatically rendered with `|_ ` per item. In single-line mode they're joined with `", "`. No extra configuration needed.

---

## Options

### Global

| Method | What it does | Default |
|---|---|---|
| `.MultiLine()` / `.SingleLine()` | Layout style | single-line |
| `.Label("…")` | Replace the class name header | class name |
| `.NoLabel()` | Hide the header entirely | — |
| `.Separator("…")` | Between properties in single-line | `", "` |
| `.CollectionSeparator("…")` | Override the auto collection separator | auto |
| `.Decimals(n)` | Decimal places for floats | `2` |
| `.Keys(NamingFormat.X)` | Key naming style | `PascalCase` |
| `.NullAs("…")` | Token used for null values | `"null"` |
| `.Only(x => x.A, x => x.B)` | Whitelist — show only these properties | all |

Collection items are rendered based on the active style: joined with `", "` in single-line, or on their own line prefixed with `|_ ` in multi-line. Multi-line items are indented to align under the prefix, so nested structures stay readable at any depth.

Available naming formats: `PascalCase`, `CamelCase`, `SnakeCase`, `KebabCase`, `HumanCase`.

### Per property

Chain `.For(x => x.Prop)` to configure a specific property, then keep chaining `.For()` to move to the next one:

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

| Method | What it does |
|---|---|
| `.Ignore()` | Exclude this property |
| `.As("…")` | Rename the key |
| `.NoKey()` | Show only the value |
| `.Prefix("…")` / `.Suffix("…")` | Wrap the value |
| `.Separator("…")` | Collection separator for this property |
| `.Decimals(n)` | Decimal places for this property |
| `.MaxItems(n)` | Truncate a collection; appends `"... and N more"` |
| `.When(v => …)` | Show only when the predicate is true |
| `.ValueFormat(v => …)` | Full control over value rendering |

`.Only()` is the shorthand for whitelisting a few properties without chaining multiple `.Ignore()` calls:

```csharp
animal.Stringify(o => o.Only(x => x.Name, x => x.Species));
// → "Animal. Name: Mittens, Species: Cat"
```

---

## Nested objects

Nested objects are rendered using the first match in this priority order:

1. A `ForNested<T>()` configuration registered on the parent
2. An overridden `ToString()` on the nested type
3. Reflection over public properties (same defaults as zero-config)

```csharp
var order = new Order { Product = new Product { Name = "Book", Price = 12.99 }, Qty = 2 };
order.Stringify();
// → "Order. Product: Product. Name: Book, Price: 12.99, Qty: 2"
```

### ForNested

Configure a nested type directly from the root call. The configuration applies wherever that type appears — as a property or inside a collection:

```csharp
zoo.Stringify(o => o
    .MultiLine()
    .ForNested<Animal>(a => a
        .NoLabel()
        .Separator(" ")
        .For(x => x.Name).NoKey()
        .For(x => x.Species).Prefix("(").Suffix(")").NoKey()
        .For(x => x.Weight).NoKey().Suffix("kg"))
    .For(x => x.EntrancePrice).Prefix("$").Decimals(0));
```

```
Zoo
EntrancePrice: $15
Animals:
|_ Mittens (Cat) 4.50kg
|_ Tony (Tiger) 120.30kg
```

### ToString()

If a nested type overrides `ToString()`, that is used automatically. If `ToString()` calls `Stringify()`, it recurses:

```csharp
public class Animal
{
    public string Name { get; set; }
    public string Species { get; set; }

    public override string ToString() => this.Stringify(o => o
        .NoLabel()
        .For(x => x.Name).NoKey()
        .For(x => x.Species).Prefix("(").Suffix(")").NoKey());
    // → "Mittens (Cat)"
}
```

---

## Migrating from the attribute API

Previous versions configured stringification via `[Stringify]`, `[StringifyProperty]`, and `[StringifyIgnore]` attributes. These still work but are deprecated and will be removed in a future major version. Move the configuration to the `.Stringify()` call site instead.

---

MIT License
