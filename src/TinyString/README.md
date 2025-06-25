# Tiny String: Small but Mighty String Utilities for .NET

![Logo](https://raw.githubusercontent.com/gianlucabelvisi/TinyTools/main/src/TinyString/logo_icon.png)

**TinyString** is a powerful, attribute-driven object pretty-printer for .NET.  
Drop it into any C# project to get beautiful, customizable string representations of your objects.

---

## Installation

```bash
dotnet add package TinyString
```

---

## Table of Contents

- [Basic Usage](#basic-usage)
- [Class-Level Customization](#class-level-customization)
- [Property-Level Customization](#property-level-customization)
- [Ignoring Properties](#ignoring-properties)
- [Naming Formats](#naming-formats)
- [Advanced Features](#advanced-features)
- [Full Examples](#full-examples)
- [License](#license)

---

## Basic Usage

Just call `.Stringify()` on any object:

```csharp
using TinyString;

public class Book
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int Pages { get; set; }
}

var book = new Book { Title = "1984", Author = "George Orwell", Pages = 328 };
Console.WriteLine(book.Stringify());
// Output: Book. Title: 1984, Author: George Orwell, Pages: 328
```

---

## Class-Level Customization

Control the output style with `[Stringify(...)]` options:

```csharp
[Stringify(
    PrintStyle = PrintStyle.MultiLine,
    Emoji = "🦁🦓🦍",
    PrintClassName = false,
    PropertySeparator = " | ",
    CollectionSeparator = "\n|_ ",
    Decimals = 0,
    NamingFormat = NamingFormat.KebabCase,
    PropertyFormat = "{k} {v}",
    ClassNameSeparator = " :: "
)]
public class Zoo
{
    public required string Title { get; set; }
    public List<Animal> Animals { get; set; } = [];
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
    [StringifyProperty(format: "{v}")]
    public required string Name { get; set; }

    [StringifyProperty(format: "({v})")]
    public required Species Species { get; set; }

    [StringifyProperty(format: "{v}kg")]
    public required double Weight { get; set; }

    [StringifyProperty(format: "{v}yrs")]
    public int Age { get; set; }

    [StringifyProperty(format: "(rare: {v})")]
    public bool IsRare { get; set; }
}

public enum Species { Cat, Tiger, Elephant }

var zoo = new Zoo
{
    Title = "Wonderful Zoo",
    EntrancePrice = 15,
    Animals = new()
    {
        new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, Age = 5, IsRare = false },
        new Animal { Name = "Tony", Species = Species.Tiger, Weight = 120.3, Age = 6, IsRare = true }
    }
};

Console.WriteLine(zoo.Stringify());
/*
🦁🦓🦍
Title Wonderful Zoo
Animals 
|_ Mittens (Cat) 4.50kg 5yrs (rare: False)
|_ Tony (Tiger) 120.30kg 6yrs (rare: True)
EntrancePrice 15
*/
```

---

## Property-Level Customization

Override formatting for individual properties:

```csharp
[StringifyProperty(format: "🎭 {v}")]
public string Name { get; set; }

[StringifyProperty(format: "💰 {v} gold")]
public decimal Gold { get; set; }

[StringifyProperty(format: "HP: {v}/{v}")]
public Health Health { get; set; }
```

---

## Ignoring Properties

Skip properties with `[StringifyIgnore]`:

```csharp
public class SecretNote
{
    public string Message { get; set; } = "";

    [StringifyIgnore]
    public string Password { get; set; } = "";
}

var note = new SecretNote { Message = "Hello", Password = "secret123" };
Console.WriteLine(note.Stringify());
// Output: SecretNote: Message: Hello
// (Password is ignored)
```

---

## Naming Formats

Choose how property names are displayed:

```csharp
[Stringify(NamingFormat = NamingFormat.SnakeCase)]
public class SnakeCaseExample
{
    public string MyProperty { get; set; } = "value";
}
// Output: SnakeCaseExample: my_property: value

[Stringify(NamingFormat = NamingFormat.HumanCase)]
public class HumanCaseExample
{
    public string MyProperty { get; set; } = "value";
}
// Output: HumanCaseExample: My Property: value
```

**Available formats:**
- `PascalCase` (default): `MyProperty`
- `CamelCase`: `myProperty`
- `SnakeCase`: `my_property`
- `KebabCase`: `my-property`
- `HumanCase`: `My Property`

---

## Advanced Features

### Nested Objects & Collections

Objects are stringified recursively, and collections are joined with your specified separator:

```csharp
[Stringify(
    PrintStyle = PrintStyle.MultiLine,
    Emoji = "⚔️",
    PropertySeparator = "\n",
    CollectionSeparator = ", ",
    Decimals = 1
)]
public class Character
{
    [StringifyProperty(format: "🎭 {v}")]
    public string Name { get; set; } = "";

    [StringifyProperty(format: "Skills: [{v}]")]
    public List<string> Skills { get; set; } = new();

    [StringifyProperty(format: "🎒 {v}")]
    public Inventory Inventory { get; set; } = new();
}

[Stringify(
    PrintStyle = PrintStyle.SingleLine,
    PropertySeparator = " | ",
    PropertyFormat = "{k}={v}"
)]
public class Inventory
{
    public List<Item> Items { get; set; } = new();
    public int Weight { get; set; }
}

[Stringify(PropertyFormat = "[{k}:{v}]")]
public class Item
{
    public string Name { get; set; } = "";
    public ItemType Type { get; set; }
}

public enum ItemType { Weapon, Armor }

var hero = new Character
{
    Name = "Gandalf",
    Skills = new() { "Fireball", "Teleport" },
    Inventory = new Inventory
    {
        Items = new() { new Item { Name = "Staff", Type = ItemType.Weapon } },
        Weight = 15
    }
};

Console.WriteLine(hero.Stringify());
/*
⚔️ Character
🎭 Gandalf
Skills: [Fireball, Teleport]
🎒 Items: [Name:Staff] [Type:Weapon] | Weight=15
*/
```

### Null & Empty Handling

```csharp
var character = new Character
{
    Name = "Empty Character",
    Skills = null!,  // null collection
    Inventory = new Inventory { Items = new() }  // empty collection
};

Console.WriteLine(character.Stringify());
// Skills: [null]
// Items: (empty collection)
```

---

## Full Examples

### RPG Character Sheet

```csharp
[Stringify(
    PrintStyle = PrintStyle.MultiLine,
    Emoji = "⚔️",
    PropertySeparator = "\n",
    CollectionSeparator = ", ",
    Decimals = 1,
    NamingFormat = NamingFormat.HumanCase
)]
public class Character
{
    [StringifyProperty(format: "🎭 {v}")]
    public string Name { get; set; } = "";

    [StringifyProperty(format: "Level {v}")]
    public int Level { get; set; }

    [StringifyProperty(format: "Class: {v}")]
    public CharacterClass Class { get; set; }

    [StringifyProperty(format: "Skills: [{v}]")]
    public List<string> Skills { get; set; } = new();

    [StringifyProperty(format: "💰 {v} gold")]
    public decimal Gold { get; set; }

    [StringifyIgnore]
    public string SecretPassword { get; set; } = "";

    [StringifyProperty(format: "⭐ {v}")]
    public bool IsLegendary { get; set; }
}

public enum CharacterClass { Warrior, Mage, Rogue }

var hero = new Character
{
    Name = "Gandalf",
    Level = 99,
    Class = CharacterClass.Mage,
    Skills = new() { "Fireball", "Teleport", "Lightning Bolt" },
    Gold = 1250.75m,
    IsLegendary = true
};

Console.WriteLine(hero.Stringify());
/*
⚔️ Character
🎭 Gandalf
Level 99
Class: Mage
Skills: [Fireball, Teleport, Lightning Bolt]
💰 1250.8 gold
⭐ True
*/
```

### Zoo Management

```csharp
[Stringify(
    Emoji = "🦁🦓🦍",
    PrintStyle = PrintStyle.MultiLine,
    CollectionSeparator = "\n|_ ",
    Decimals = 0
)]
public class Zoo
{
    public string Title { get; set; } = "";
    public List<Animal> Animals { get; set; } = new();
    public double EntrancePrice { get; set; }
}

[Stringify(
    PrintStyle = PrintStyle.SingleLine,
    PropertySeparator = " ",
    Decimals = 2
)]
public class Animal
{
    [StringifyProperty(format: "{v}")]
    public string Name { get; set; } = "";

    [StringifyProperty(format: "({v})")]
    public Species Species { get; set; }

    [StringifyProperty(format: "{v}kg")]
    public double Weight { get; set; }

    [StringifyProperty(format: "{v}yrs")]
    public int Age { get; set; }
}

public enum Species { Cat, Tiger, Elephant }

var zoo = new Zoo
{
    Title = "Wonderful Zoo",
    Animals = new()
    {
        new Animal { Name = "Mittens", Species = Species.Cat, Weight = 4.5, Age = 5 },
        new Animal { Name = "Tony", Species = Species.Tiger, Weight = 120.3, Age = 6 }
    },
    EntrancePrice = 15
};

Console.WriteLine(zoo.Stringify());
/*
🦁🦓🦍 Zoo
Title: Wonderful Zoo
Animals: 
|_ Mittens (Cat) 4.50kg 5yrs
|_ Tony (Tiger) 120.30kg 6yrs
EntrancePrice: 15
*/
```

---

## License

TinyString is licensed under the MIT License.