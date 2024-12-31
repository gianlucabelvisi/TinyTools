# TinyOptional: A Lightweight C# "Maybe" / Optional Type

This library provides a simple **Optional** (a.k.a. “Maybe”) type for C#. It allows you to express the presence or absence of a value without resorting to `null`. If an `Optional<T>` has a value, you can safely work with that value; if it doesn’t, the library’s methods help gracefully handle the “no value” scenario.

## Table of Contents
- [Why Use an Optional Type?](#why-use-an-optional-type)
- [Key Features](#key-features)
- [Installation](#installation)
- [Getting Started](#getting-started)
  - [Creating Optionals](#creating-optionals)
  - [Checking for a Value](#checking-for-a-value)
  - [Retrieving the Value](#retrieving-the-value)
  - [Supplying a Default](#supplying-a-default)
  - [Filtering with `Where`](#filtering-with-where)
  - [Mapping with `Select`](#mapping-with-select)
  - [Conditional Actions: `IfPresent` / `IfNotPresent`](#conditional-actions-ifpresent--ifnotpresent)
- [Extension Methods](#extension-methods)
  - [Collections](#collections)
  - [Strings](#strings)
- [License](#license)

---

## Why Use an Optional Type?

- **Clarity**: Instead of passing `null` or returning `null`, an `Optional<T>` signals explicitly that the result might be absent.  
- **Safety**: Avoid `NullReferenceException` by forcing the developer to deal with the possibility of an empty value.  
- **Functional Style**: `Where`, `Select`, `SelectMany`, `IfPresent`, etc., allow fluent transformations and checks.

---

## Key Features

- **`Of`, `OfNullable`, `Empty`**: Create an `Optional<T>` with or without an initial value.  
- **Checking**: `IsPresent()`, `IsNotPresent()`.  
- **Retrieving**: `Get()`, `GetOrThrow(exceptionSupplier)`, `OrElse(...)`, etc.  
- **Filtering**: `Where(predicate)` returns an empty `Optional` if the predicate fails.  
- **Transforming**: `Select(...)`, `SelectMany(...)`.  
- **Conditional Actions**: `IfPresent(...)`, `IfNotPresent(...)`, including async versions.  
- **Extension Methods**: `FirstIfExists`, `LastIfExists` for collections, `IfAny` for strings, etc.

---

## Installation

```bash
  dotnet add package TinyOptional
```

---

## Getting Started

### Creating Optionals

```csharp

// Throws an ArgumentNullException if `value` is null
var maybeTen = Optional<int>.Of(10);

// Allows null but returns an empty Optional
var maybeValue = Optional<string>.OfNullable(someString);

// A straightforward empty Optional
var none = Optional<int>.Empty();
```

### Unboxing an optional

```csharp
var tenMaybe = Optional<int>.Of(10);

# check if the value is present
bool hasValue = tenMaybe.IsPresent();   
bool noValue = tenMaybe.IsNotPresent();

# getting the value
if (tenMaybe.IsPresent())
{
    int ten = tenMaybe.Get();
}
```

While possible, this is not the suggested workflow, as it would be a glorified, more verbose null-check. Instead, use the methods below.

### Unboxing with fallback or error handling

```csharp
var empty = Optional<int>.Empty();

// Returns 42 if the Optional is empty
int value = empty.OrElse(42);

// In case the fallback is expensive to compute
int value2 = empty.OrElseGet(() => ComputeFallback(42));

// In case no fallback is possible
int value3 = empty.OrElseThrow(() => new Exception("No value available"));

```

### Filtering with `Where`

It is possible to apply a where clause to an Optional, returning an empty Optional if the predicate fails.

```csharp
var maybeTen = Optional<int>.Of(10);

// Returns an empty Optional if the value is not greater than 5
var filtered = maybeTen.Where(value => value > 5);
```

### Mapping with `Select`

The content of an optional can be transformed using the `Select` method, before being boxed back into an Optional and eventually unboxed.
    
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

### Conditional Actions: `IfPresent` / `IfNotPresent`

The `IfPresent` and `IfNotPresent` methods allow you to execute an action if the Optional has a value or not, respectively.

```csharp
var maybeTen = Optional<int>.Of(10);

maybeTen
    .IfPresent(value => Console.WriteLine($"The value is {value}"))
    .OrElse(() => Console.WriteLine("No value"));
```

---

## Extension Methods

### Collections

The library provides a few extension methods to work with collections of Optionals.

```csharp
var fibonacci = new List<int>() = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89 };

var firstEven = fibonacci
  .FirstIfExists(optional => optional.Where(value => value % 2 == 0))
  .OrElse(0);
```

### Strings

The library provides a few extension methods to work with strings.

```csharp
  var inputName = ReadInputName();
  
    var name = inputName
        .IfAny()
        .OrElse("No name provided");
```

---

## License

TinyOptional is licensed under the GPL v3. By using or redistributing this library, you agree to comply with the terms of that license.
```
