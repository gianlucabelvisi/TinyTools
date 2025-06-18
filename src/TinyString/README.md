# Tiny String: Small but Mighty String Utilities for .NET

![Logo](https://raw.githubusercontent.com/gianlucabelvisi/TinyTools/main/src/TinyString/logo.png)

**TinyString** is a grab-bag of lightweight, self-contained string utilities and a powerful, attribute-driven object pretty-printer for .NET.  
Drop it into any C# project to get:

- Case converters: `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`, `ToHumanCase`
- Slugging, digit helpers, safe joins, and more
- A flexible `.Stringify()` extension for beautiful, customizable object printing

---

## Installation

```bash
dotnet add package TinyString
```

---

## Table of Contents

- [Object Pretty-Printing with Stringify](#object-pretty-printing-with-stringify)
  - [Basic Usage](#basic-usage)
  - [Class-Level Customization](#class-level-customization)
  - [Property-Level Customization](#property-level-customization)
  - [Ignoring Properties](#ignoring-properties)
  - [Naming Formats](#naming-formats)
  - [Advanced: Nested Objects, Collections, Nulls](#advanced-nested-objects-collections-nulls)
- [Full Example: RPG Character Sheet](#full-example-rpg-character-sheet)
- [License](#license)

---

## Object Pretty-Printing with Stringify

### Basic Usage

Just add `[Stringify]` to your class and call `.Stringify()`:

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
// Output: Book: Title: 1984, Author: George Orwell, Pages: 328
```

### Class-Level Customization

Control the output style with `[Stringify(...)]` options:

```csharp
[Stringify(
    PrintStyle = PrintStyle.MultiLine,
    Emoji = "📚",
    PrintClassName = false,
    PropertySeparator = " | ",
    CollectionSeparator = "\n- ",
    Decimals = 1,
    NamingFormat = NamingFormat.KebabCase,
    Format = "[{k}] = {v}",
    ClassNameSeparator = " :: "
)]
public class Library
{
    public string Name { get; set; } = "";
    public List<Book> Books { get; set; } = new();
    public double Rating { get; set; }
}

var library = new Library
{
    Name = "City Library",
    Books = new() { book },
    Rating = 4.8
};
Console.WriteLine(library.Stringify());
/*
📚
Name: City Library
Books: 
- Book: Title: 1984, Author: George Orwell, Pages: 328
Rating: 4.8
*/
```

### Property-Level Customization

Override formatting for individual properties:

```csharp
public class Movie
{
    public string Title { get; set; } = "";

    [StringifyProperty(format: "\"{v}\"")]
    public string Director { get; set; } = "";

    [StringifyProperty(format: "{k} ({v} min)")]
    public int Duration { get; set; }
}
```

### Ignoring Properties

Skip properties with `[StringifyIgnore]`:

```csharp
public class SecretNote
{
    public string Message { get; set; } = "";

    [StringifyIgnore]
    public string Password { get; set; } = "";
}
```

### Naming Formats

Choose how property names are displayed:

- `PascalCase` (default): `MyProperty`
- `CamelCase`: `myProperty`
- `SnakeCase`: `my_property`
- `KebabCase`: `my-property`
- `HumanCase`: `My Property`

```csharp
[Stringify(NamingFormat = NamingFormat.SnakeCase)]
public class SnakeCaseExample
{
    public string MyProperty { get; set; } = "value";
}
// Output: SnakeCaseExample: my_property: value
```

### Advanced: Nested Objects, Collections, Nulls

- Nested objects are stringified recursively.
- Collections are joined with the specified separator.
- Nulls are printed as `"null"`.

```csharp
public class Team
{
    public string Name { get; set; } = "";
    public List<Person> Members { get; set; } = new();
}

public class Person
{
    public string Name { get; set; } = "";
    public int? Age { get; set; }
}

var team = new Team
{
    Name = "Avengers",
    Members = new() { new Person { Name = "Tony", Age = 48 }, new Person { Name = "Steve", Age = null } }
};
Console.WriteLine(team.Stringify());
/*
Team: Name: Avengers, Members: Person: Name: Tony, Age: 48; Person: Name: Steve, Age: null
*/
```

---

## Full Example: RPG Character Sheet

```csharp
[Stringify(
    PrintStyle = PrintStyle.MultiLine,
    Emoji = \"🧙‍♂️\",
    Format = \"{k} => {v}\",
    PropertySeparator = \"\\n\",
    CollectionSeparator = \", \"
)]
public class Character
{
    public string Name { get; set; } = \"\";
    public int Level { get; set; }
    public List<string> Skills { get; set; } = new();
    public Stats Attributes { get; set; } = new();
}

[Stringify(Format = \"{k}: {v}\")]
public class Stats
{
    public int Strength { get; set; }
    public int Intelligence { get; set; }
    public int Dexterity { get; set; }
}

var hero = new Character
{
    Name = \"Merlin\",
    Level = 99,
    Skills = new() { \"Fireball\", \"Teleport\" },
    Attributes = new Stats { Strength = 10, Intelligence = 20, Dexterity = 15 }
};
Console.WriteLine(hero.Stringify());
/*
🧙‍♂️
Name => Merlin
Level => 99
Skills => Fireball, Teleport
Attributes => Stats: Strength: 10, Intelligence: 20, Dexterity: 15
*/
```

---

## License

TinyString is licensed under the MIT License.

---

Would you like to proceed with this structure and content, or do you want to add/change any sections or example models?