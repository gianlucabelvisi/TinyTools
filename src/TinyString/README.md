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
| `.ReplaceValues(dict)` | Swap specific values for custom strings |
| `.DateFormat("…")` | Format `DateTime` / `DateTimeOffset` values |
| `.BoolAs("Yes", "No")` | Custom labels for `true` / `false` |
| `.Truncate(n)` | Cap string length, appends `"…"` |
| `.Transform(v => …)` | Transform the value before rendering |
| `.UpperCase()` / `.LowerCase()` | Case conversion shortcuts |
| `.NullAs("…")` | Per-property null token (overrides global) |
| `.NumberFormat("…")` | .NET format string for numeric values |
| `.Highlight(pred, fmt)` | Conditional formatting when predicate matches |

`.Only()` is the shorthand for whitelisting a few properties without chaining multiple `.Ignore()` calls:

```csharp
animal.Stringify(o => o.Only(x => x.Name, x => x.Species));
// → "Animal. Name: Mittens, Species: Cat"
```

---

## Value formatting

### ReplaceValues — swap specific values for labels or emoji

Map known values to display strings. Works with any type — strings, ints, enums, etc. Values not in the dictionary render normally.

Matching is done against the rendered string representation, so for complex types (classes), the replacement matches against `ToString()`.

```csharp
task.Stringify(o => o
    .For(x => x.Source).ReplaceValues(new() {
        { "area", "🗺️" },
        { "inbox", "📥" }
    })
    .For(x => x.Priority).ReplaceValues(new() {
        { 1, "🔴 Critical" },
        { 2, "🟡 Medium" },
        { 3, "🟢 Low" }
    }));
// → "Task. Title: Fix bug, Source: 📥, Priority: 🔴 Critical"
```

For nested objects with `ToString()` overrides:

```csharp
// Given: AnimalWithToString.ToString() → "Mittens (Cat)"
task.Stringify(o => o
    .For(x => x.Owner).ReplaceValues(new() {
        { new Animal { Name = "Mittens", Species = "Cat" }, "🐱 Mittens" }
    }));
// → "Task. Title: Fix bug, Owner: 🐱 Mittens"
```

### DateFormat — format dates and times

Apply a .NET format string to `DateTime` or `DateTimeOffset` properties.

```csharp
event.Stringify(o => o
    .For(x => x.StartsAt).DateFormat("yyyy-MM-dd")
    .For(x => x.CreatedAt).DateFormat("dd MMM yyyy HH:mm"));
// → "Event. Name: Launch, StartsAt: 2026-03-15, CreatedAt: 01 Jun 2026 09:00"
```

### BoolAs — friendly true/false labels

Replace `True` / `False` with any pair of strings.

```csharp
animal.Stringify(o => o
    .For(x => x.IsRare).BoolAs("✅", "❌"));
// → "Animal. Name: Mittens, ..., IsRare: ❌"

user.Stringify(o => o
    .For(x => x.IsActive).BoolAs("Yes", "No"));
// → "User. Name: Alice, IsActive: Yes"
```

### Truncate — cap long strings

Cut values longer than *n* characters and append `…`.

```csharp
article.Stringify(o => o
    .For(x => x.Body).Truncate(50));
// → "Article. Title: Hello, Body: Lorem ipsum dolor sit amet, consectetur adipisci…"
```

### Transform / UpperCase / LowerCase

Transform the value *before* it goes through normal rendering. The result is then converted to a string using the standard rules.

```csharp
animal.Stringify(o => o
    .For(x => x.Name).UpperCase()
    .For(x => x.Species).LowerCase());
// → "Animal. Name: MITTENS, Species: cat, ..."

// Custom transform — any function that returns a new value:
order.Stringify(o => o
    .For(x => x.Tags).Transform(tags => string.Join(", ", tags.Select(t => $"#{t}"))));
```

### Per-property NullAs

Override the global null token for a single property.

```csharp
sparse.Stringify(o => o
    .NullAs("—")                       // global default
    .For(x => x.Name).NullAs("N/A")); // just for Name
// → "Sparse. Name: N/A, Description: —, Count: 3"
```

### NumberFormat — .NET numeric format strings

Use any standard or custom .NET numeric format: `"N0"` for thousands separators, `"P1"` for percentage, `"C"` for currency, `"D6"` for zero-padded integers, etc.

```csharp
invoice.Stringify(o => o
    .For(x => x.Amount).NumberFormat("N2")
    .For(x => x.TaxRate).NumberFormat("P0")
    .For(x => x.Id).NumberFormat("D6"));
// → "Invoice. Id: 001234, Amount: 12,345.68, TaxRate: 21 %"
```

### Highlight — conditional formatting

Apply a custom formatter only when a condition is met. Values that don't match the predicate render normally.

```csharp
invoice.Stringify(o => o
    .For(x => x.Amount).Highlight(
        v => v > 100,
        v => $"⚠️ {v:F0}"));
// amount = 150 → "Amount: ⚠️ 150"
// amount = 50  → "Amount: 50.00"
```

### Combining features

All per-property features compose naturally:

```csharp
task.Stringify(o => o
    .For(x => x.Source)
        .ReplaceValues(new() { { "inbox", "Inbox" } })
        .Prefix("[").Suffix("]"))
    .For(x => x.Title).UpperCase().Truncate(20));
// → "Task. Title: FIX THE CRITICAL BU…, Source: [Inbox], Priority: 1"
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

MIT License
