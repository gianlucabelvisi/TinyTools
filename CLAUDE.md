# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

TinyTools is a collection of small .NET libraries published to NuGet:
- **TinyOptional** (v2.x): A `Optional<T>` / Maybe monad implementation
- **TinyString** (v0.x): A reflection-based pretty-printer with attribute-driven customization

Both libraries target `net8.0` and `net9.0`. Tests use NUnit 4 with FluentAssertions.

## Commands

```bash
# Build
dotnet build
dotnet build -c Release

# Test all
dotnet test

# Test a single project
dotnet test test/TinyOptionalTests/
dotnet test test/TinyStringTests/

# Run a specific test by name
dotnet test --filter "TestName"

# Publish to NuGet (done via GitHub Actions manually)
# Trigger workflow_dispatch on .github/workflows/publishTinyOptional.yaml or publishTinyString.yaml
```

## Versioning

Each library has its own `Version.props` file at `src/<Library>/Version.props`. Update that file when bumping versions — the `.csproj` references `$(ProjectVersion)` from it. The two libraries version independently.

## Architecture

### TinyString

Reflection-based pretty-printer. Entry point is `Stringifier.cs`, which adds the `.Stringify()` extension method to any object. Key design:

- Reads public properties via reflection at call time (no caching)
- Behavior customized via three attributes: `[Stringify(...)]` (class-level), `[StringifyProperty(...)]` (property-level), `[StringifyIgnore]` (skip)
- Naming format conversions (CamelCase, SnakeCase, etc.) live in `StringExtensions.cs`
- SmartEnum types are detected by checking for a `Name` property — not by interface
- `PrintStyle.MultiLine` outputs one property per line; `SingleLine` (default) comma-separates

### TinyOptional

`Optional<T>` wraps a value that may or may not be present. Key design:

- Private constructor — created only via `Of()`, `OfNullable()`, or `Empty()`
- `Of()` throws `ArgumentNullException` on null; `OfNullable()` wraps null as empty
- Chaining: `IfPresent()` / `IfNotPresent()` return intermediate result objects to allow `.OrElse()`-style chaining without nesting
- Collection extensions (`FirstIfExists`, `LastIfExists`, `ElementAtIfExists`) live in `OptionalExtensions.cs`
