# GitHub Copilot Instructions for Plugin.BaseTypeExtensions

## Project Overview

**Plugin.BaseTypeExtensions** is a collection of extension methods for .NET base types —
strings, numerics, enums, collections, dates/times, comparables, reflection, and a handful of
other primitives — aimed at productivity, type safety, and readability in modern .NET
applications.

**Key Features:**
- Null-safe string operations and character manipulation
- Generic numeric operations for all `INumber<T>` types (angles, percentages, clamping)
- Enum flag manipulation, description retrieval, and general enum utilities
- Dictionary/list/enumerable extensions, including synchronization-aware variants
- `DateTime`/`DateOnly`/`TimeOnly`/`TimeSpan` utilities
- Reflection and assembly helpers (type discovery, attribute scanning, embedded resources)
- `IComparable<T>` extensions plus standalone `ComparableTools`/`NumericRangeTools` helpers
- Smaller extensions for `Byte`, `Guid`, `Version`, `Uri`, `Task`, `TaskCompletionSource`,
  `CancellationToken`, `SemaphoreSlim`, `Random`, `Exception`

## Technology Stack

- **Framework:** .NET 9.0 (SDK `9.0.305`, see `global.json`)
- **Language:** C# (latest)
- **Testing:** xUnit + FluentAssertions (v7.x — stays open source indefinitely per Directory.Packages.props comment)
- **Package Management:** Central Package Management (`Directory.Packages.props`)
- **CI/CD:** GitHub Actions
- **Documentation:** DocFX (`Plugin.BaseTypeExtensions.Docs/`, published to GitHub Pages)

## Project Structure

```
Plugin.BaseTypeExtensions/
├── .github/
│   ├── workflows/ci.yml
│   └── copilot-instructions.md   # This file
├── Plugin.BaseTypeExtensions/          # Main library — one file per extended type/category
│   ├── StringExtensions.cs
│   ├── NumericExtensions.cs / NumericRangeTools.cs
│   ├── EnumExtensions.cs
│   ├── DictionaryExtensions.cs / ListExtensions.cs / EnumerableExtensions.cs
│   ├── ConcurrentDictionaryExtensions.cs / SemaphoreSlimExtensions.cs
│   ├── DateTimeExtensions.cs / DateOnlyExtensions.cs / TimeOnlyExtensions.cs / TimeSpanExtensions.cs
│   ├── ComparableExtensions.cs / ComparableTools.cs
│   ├── ReflectionExtensions.cs / AssemblyExtensions.cs
│   ├── ByteExtensions.cs / GuidExtensions.cs / VersionExtensions.cs / UriExtensions.cs
│   ├── TaskExtensions.cs / TaskCompletionSourceExtensions.cs / CancellationTokenExtensions.cs
│   ├── RandomExtensions.cs / ExceptionExtensions.cs
├── Plugin.BaseTypeExtensions.Tests/    # xUnit tests, one file per source file
└── Plugin.BaseTypeExtensions.Docs/     # DocFX documentation site
```

## Development Setup

### Prerequisites
- .NET SDK 9.0.305 or later (see `global.json`)

### Build Commands
```bash
dotnet restore Plugin.BaseTypeExtensions.slnx
dotnet build Plugin.BaseTypeExtensions.slnx --configuration Release --no-restore
dotnet test Plugin.BaseTypeExtensions.Tests/Plugin.BaseTypeExtensions.Tests.csproj --configuration Release --no-build
```

## Code Organization Patterns

- **One file per extended type or tightly-related group** — `*Extensions.cs`, named after the
  type it extends (e.g. `StringExtensions.cs`). Don't mix categories in one file.
- **Generic numeric code uses `INumber<T>`** constraints instead of per-type overloads.
- **Standalone helpers that don't read naturally as extension methods** (e.g. clamping across a
  pair of bounds) live in `ComparableTools`/`NumericRangeTools` as static classes — keep that
  separation rather than bolting them onto `ComparableExtensions`/`NumericExtensions`.

## Coding Standards

### Naming Conventions (enforced by `.editorconfig`)
- Classes/structs/interfaces/methods/properties/constants: PascalCase
- Private fields: `_camelCase`
- Parameters/locals: camelCase

### Style Guidelines
- Line length: max 240 characters; indentation: 4 spaces; line endings: LF
- Prefer `var` when the type is apparent
- Braces always required, even for single-line statements
- `System` usings first, separated from the rest

### Documentation Requirements
- XML docs (`<summary>`, `<param>`, `<returns>`, `<exception>`) required on all public APIs

## Testing Guidelines

- **Framework:** xUnit, assertions via FluentAssertions (v7.2.2, open-source)
- **File naming:** `{ClassName}Tests.cs`, matching the source file
- **Test naming:** `MethodName_Scenario_ExpectedBehavior`
- Cover success paths, failure paths, and boundary conditions (null, empty, min/max values)
- Use `[Theory]`/`[InlineData]` for multiple similar cases; keep tests independent of each other

## Dependencies

### Production
- `Microsoft.Extensions.Logging.Abstractions` (10.0.10)

### Development
- `xunit` (2.9.3), `xunit.runner.visualstudio` (3.1.5), `FluentAssertions` (7.2.2)
- `coverlet.collector` / `coverlet.msbuild` (10.0.1)
- `Microsoft.NET.Test.Sdk` (18.8.1), `JetBrains.Annotations` (2025.2.0)
- `Microsoft.CodeAnalysis.NetAnalyzers` (10.0.302), `Microsoft.SourceLink.GitHub` (10.0.201)

(Versions drift via Dependabot — check `Directory.Packages.props` for current values rather than
trusting this list long-term.)

## CI/CD Pipeline

`ci.yml` runs: version generation → build/test/package → publish test artifacts → DocFX build and
deploy to GitHub Pages → publish to NuGet.org (on `main`) → tag & GitHub release.

## Useful Resources

- **Repository:** https://github.com/laerdal/Plugin.BaseTypeExtensions
- **NuGet Package:** https://www.nuget.org/packages/Plugin.BaseTypeExtensions
- **Documentation:** https://laerdal.github.io/Plugin.BaseTypeExtensions/
