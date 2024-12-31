# Tiny Optional: A Lightweight C# Optional Type

This library provides a simple **Optional** (a.k.a. “Maybe”) type for C#. It allows you to express the presence or absence of a value without resorting to `null`. If an `Optional<T>` has a value, you can safely work with that value; if it doesn’t, the library’s methods help gracefully handle the “no value” scenario.

## Why Use an Optional Type?

- **Clarity**: Instead of passing `null` or returning `null`, an `Optional<T>` signals explicitly that the result might be absent.  
- **Safety**: Avoid `NullReferenceException` by forcing the developer to deal with the possibility of an empty value.  
- **Functional Style**: `Where`, `Select`, `SelectMany`, `IfPresent`, etc., allow fluent transformations and checks.


## Installation

```bash
dotnet add package TinyOptional
```

## Getting Started

### Creating Optionals

```csharp

// Throws an ArgumentNullException if `value` is null
var maybeTen = Optional<int>.Of(10);

// Allows null but returns an empty Optional
var maybeName = Optional<string>.OfNullable(userName);

// A straightforward empty Optional
var none = Optional<int>.Empty();
```

### Unboxing an optional

This is a simple and **wrong** way of unboxing an optional:

```csharp
var maybeName = Optional<string>.OfNullable(userName);

if (maybeName.IsPresent())
{
    var name = maybeName.Get();
}
```

While possible, this is not the suggested workflow, as it would be a glorified, more verbose null-check. 

Instead...

### Unboxing with fallback or error handling

```csharp
var maybeName = Optional<string>.OfNullable(userName);

// Simple fallback
var name = maybeName.OrElse("No name provided");

// In case the fallback is expensive to compute
var name2 = maybeName.OrElseGet(() => GenerateRandomName());

// In case no fallback is possible
var name3 = empty.OrElseThrow(() => new Exception("Name is required"));
```

### Filtering and mapping 

It is possible to apply a `Where` clause to an Optional, returning empty if the predicate fails.

The content of an optional can then be transformed using the `Select` method, before being boxed back into an Optional.

This allows for a fluent, style of programming where the presence of the content is not required for the operations on it to take place.
    
```csharp

var maybeUniverse = Optional<Universe>.Of(new Universe()
{
    Age = 13.8
    AgeUnit = "billion years"
});

var age = maybeUniverse
    .Where(u => u.AgeUnit == "billion years")
    .Whre(u => u.Age > 13)
    .Select(u => u.Age)
    .OrElseThrow(() => new Exception("No universe found"));
```

### Conditional Actions

The `IfPresent` and `IfNotPresent` methods allow you to execute an action if the Optional has a value or not, respectively.

```csharp
var maybeTen = Optional<int>.Of(10);

maybeTen
    .IfPresent(value => Console.WriteLine($"The value is {value}"))
    .OrElse(() => Console.WriteLine("No value"));
```

### Collections

A version of FirstOrDefault that returns an Optional instead of null.

```csharp
var fibonacci = new List<int>() = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };

var firstEven = fibonacci
  .FirstIfExists(optional => optional.Where(value => value % 2 == 0))
  .OrElse(0);
```

### Strings

```csharp
  var inputName = ReadInputName();
  
    var name = inputName
        .IfAny()
        .OrElse("No name provided");
```

## License

TinyOptional is licensed under the GPL v3. By using or redistributing this library, you agree to comply with the terms of that license.

