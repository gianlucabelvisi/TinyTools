# TinyOptional

![Logo](https://raw.githubusercontent.com/gianlucabelvisi/TinyTools/main/src/TinyOptional/logo_icon.png)

Express the presence or absence of a value without `null`. Zero ceremony, clean chaining, no exceptions by surprise.

```bash
dotnet add package TinyOptional
```

---

## Creating an Optional

```csharp
using TinyOptional;

Optional<string>.Of("hello")           // non-null value — throws if null
Optional<string>.OfNullable(maybeNull) // wraps null as empty
Optional<string>.Empty()               // explicitly empty
```

---

## Getting the value out

```csharp
optional.OrElse("default")                      // fallback value
optional.OrElseGet(() => ComputeDefault())       // fallback computed lazily
optional.OrElseNull()                            // returns T? — null if empty
optional.OrElseDo(() => Log("nothing here"))     // run side effect if empty
optional.Get()                                   // throws if empty
optional.GetOrThrow(() => new NotFoundException())
```

---

## Transforming

```csharp
optional
    .Where(x => x.Length > 0)           // filter — empty if predicate fails
    .Select(x => x.ToUpper())           // map — stays empty if already empty
    .SelectMany(x => Parse(x))          // flatmap — avoids Optional<Optional<T>>
    .OrElse("FALLBACK")
```

Async variants: `.WhereAsync(...)`, `.SelectAsync(...)`

---

## Collapsing to a value

```csharp
var label = optional.Match(
    onPresent: name => $"Hello, {name}",
    onEmpty:   ()   => "Hello, stranger"
);
```

---

## Side effects

```csharp
optional
    .IfPresent(v => Console.WriteLine($"Got: {v}"))
    .OrElse(() => Console.WriteLine("Nothing"));

optional
    .IfNotPresent(() => Console.WriteLine("Nothing"))
    .OrElse(v => Console.WriteLine($"Got: {v}"));
```

Async variants: `.IfPresentAsync(...)`, `.IfNotPresentAsync(...)`

---

## LINQ integration

```csharp
optional.ToEnumerable()  // IEnumerable<T> with 0 or 1 element

// works in query expressions and SelectMany chains
var results = from opt in listOfOptionals
              from value in opt.ToEnumerable()
              select value;
```

---

## Equality

Two optionals are equal when both are empty, or both are present with equal values:

```csharp
Optional<int>.Of(42) == Optional<int>.Of(42)  // true
Optional<int>.Empty() == Optional<int>.Empty() // true
Optional<int>.Of(1)  == Optional<int>.Of(2)   // false
```

---

## Collection extensions

All return `Optional<T>` instead of throwing or returning null:

```csharp
list.FirstIfExists()                    // first element, or empty
list.FirstIfExists(x => x.IsActive)    // first match, or empty
list.LastIfExists()                     // last element, or empty
list.LastIfExists(x => x.IsActive)     // last match, or empty
list.SingleIfExists()                   // the only element — empty if 0 or 2+
list.SingleIfExists(x => x.IsActive)   // the only match — empty if 0 or 2+
list.ElementAtIfExists(3)              // safe index access
list.AggregateIfExists(seed, func)     // empty if source is null or empty
```

---

## String extension

```csharp
"hello".IfAny()   // Optional<string> with value
"".IfAny()        // empty Optional
((string?)null).IfAny()  // empty Optional
```

---

MIT License
