# 🔧 Plugin.BaseTypeExtensions

<div style="max-width: 256px; margin-left: auto; margin-right: auto;">

[![Icon](icon.png)](https://github.com/laerdal/Plugin.ByteArrays)

</div>

[![CI](https://img.shields.io/github/actions/workflow/status/laerdal/Plugin.BaseTypeExtensions/ci.yml?logo=github)](https://github.com/laerdal/Plugin.BaseTypeExtensions/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Plugin.BaseTypeExtensions?logo=nuget&color=004880)](https://www.nuget.org/packages/Plugin.BaseTypeExtensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Plugin.BaseTypeExtensions?logo=nuget&color=004880)](https://www.nuget.org/packages/Plugin.BaseTypeExtensions)
[![GitHub Release](https://img.shields.io/github/v/release/laerdal/Plugin.BaseTypeExtensions?logo=github)](https://github.com/laerdal/Plugin.BaseTypeExtensions/releases)
[![License](https://img.shields.io/github/license/laerdal/Plugin.BaseTypeExtensions?color=blue)](LICENSE.md)
[![GitHub Pages](https://img.shields.io/badge/docs-GitHub%20Pages-blue?logo=github)](https://laerdal.github.io/Plugin.BaseTypeExtensions/)

A comprehensive collection of extension methods for .NET base types, designed to enhance productivity, type safety, and code readability in modern .NET applications.

---

## 📦 Features

- **String Extensions**: Null-safe string operations and character manipulation
- **Numeric Extensions**: Generic numeric operations for all `INumber<T>` types including angles, percentages, and clamping
- **Enum Extensions**: Flag manipulation, description retrieval, and comprehensive enum utilities
- **Collection Extensions**: Advanced dictionary, list, and enumerable operations with synchronization support
- **Temporal Extensions**: Date and time utilities for `DateTime`, `DateOnly`, and `TimeOnly`
- **Reflection & Assembly**: Type discovery, attribute scanning, and embedded resource management
- **Comparison Extensions**: Enhanced operations for `IComparable<T>` types
- **Specialized Utilities**: Extensions for `Byte`, `Guid`, `Version`, `Task`, and `Random` types

---

## 🛠️ Usage Examples

### String Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Null-safe string operations
string? input = GetUserInput();
string result = input.NullIfEmptyOrWhiteSpace() ?? "default value";

// Character filtering
string clean = "Hello@World!#123".RemoveSpecialCharacters(); // "HelloWorld123"
```

### Numeric Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Angle conversions (works with any INumber<T>)
double radians = 45.0.DegreeToRadian();
float degrees = 1.57f.RadianToDegree();

// Range operations
int clamped = value.Clamp(0, 100);
double percentage = 75.0.PercentageToValue(0.0, 200.0); // 150.0
byte normalized = 0.75.PercentageToByte(); // 191

// Custom numeric ranges
var range = NumericRangeTools.GetRange(0.0, 10.0, 0.5);
foreach (var value in range) { /* 0.0, 0.5, 1.0, ... 10.0 */ }
```

### Enum Extensions

```csharp
using Plugin.BaseTypeExtensions;

[Flags]
public enum FilePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}

// Flag operations
var permissions = FilePermissions.None
    .SetFlag(FilePermissions.Read)
    .SetFlag(FilePermissions.Write);

bool canWrite = permissions.HasFlag(FilePermissions.Write);
permissions = permissions.UnsetFlag(FilePermissions.Write);

// Get all enum values
var allPermissions = EnumExtensions.AllAsArray<FilePermissions>();

// Description support
[Description("Read access to files")]
public enum Access { [Description("Read access")] Read }
string desc = Access.Read.GetDescription(); // "Read access"
```

### Collection Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Dictionary operations
var dict = new Dictionary<string, int>();
dict.Add(new KeyValuePair<string, int>("key", 42));
dict.Update(new KeyValuePair<string, int>("key", 100));

// Collection synchronization
sourceList.UpdateFrom(
    targetList,
    matcher: (s, t) => s.Id == t.Id,
    onAdd: item => Console.WriteLine($"Added: {item}"),
    onRemove: item => Console.WriteLine($"Removed: {item}")
);

// Safe enumerable operations
var items = GetItems();
var count = items.NullIfEmpty()?.Count(); // null if empty, count otherwise
var item = items.GetOrDefault(index: 5, defaultValue: new Item());

// Queue with size limit
var queue = new ConcurrentQueue<string>();
queue.Enqueue("item", maxSize: 10); // automatically dequeues if over limit
```

### Temporal Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Date range calculations
var start = new DateTime(2024, 1, 1);
var end = new DateTime(2024, 12, 31);
var current = new DateTime(2024, 6, 15);

double progress = current.DateTimeToPercentage(start, end); // ~0.5
var halfwayPoint = 0.5.PercentageToDateTime(start, end);

// Works with DateOnly and TimeOnly too
var dateProgress = DateOnly.FromDateTime(current)
    .DateOnlyToPercentage(DateOnly.FromDateTime(start), DateOnly.FromDateTime(end));
```

### Reflection & Assembly Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Get assembly from instance
var assembly = myObject.GetAssembly();

// Find types with attributes
var typesWithAttribute = assembly.GetTypesWithAttribute<ServiceAttribute>();
foreach (var (attribute, type) in typesWithAttribute)
{
    Console.WriteLine($"Service: {type.Name}");
}

// Async version available
var asyncTypes = await assembly.GetTypesWithAttributeAsync<ServiceAttribute>();

// Extract embedded resources
var resourceFile = assembly.MoveManifestResourceToCache(
    "MyApp.Resources.config.json",
    filename: "app-config.json",
    fileAlreadyExistsBehavior: FileAlreadyExistsBehavior.Overwrite
);

// Async resource extraction
var asyncResource = await assembly.MoveManifestResourceToDirectoryAsync(
    "MyApp.Resources.template.html",
    targetDirectory: "/tmp/app-resources"
);
```

### Comparison Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Fluent comparisons
bool inRange = value.IsBetween(min, max, inclusive: true);
bool isGreater = score.IsGreaterThan(threshold);
bool isValid = price.IsGreaterThanOrEqual(0);

// Utility functions
var minimum = ComparableTools.Min(value1, value2);
var maximum = ComparableTools.Max(value1, value2);
```

### Specialized Extensions

```csharp
using Plugin.BaseTypeExtensions;

// Byte operations
byte value = 0xAB;
string bits = value.ToBitsString(); // "10101011"
byte reversed = value.ToReversedByte(); // 0xD5
byte upperNibble = value.GetUpperNibbleBitsAsNewByte(); // 0xA0

// GUID utilities
var guid = Guid.NewGuid();
var nextGuid = guid.GetPlusOne(); // Sequential GUID

// Version operations
var version = new Version(1, 2, 3);
// Version comparison and manipulation methods available

// Task extensions
var tcs = new TaskCompletionSource<string>();
// Enhanced TaskCompletionSource operations

// Random extensions
var random = new Random();
// Additional random generation methods
```

---

## 📋 API Overview

### Core Extension Categories

- **`StringExtensions`** - Null-safe operations and character manipulation
- **`NumericExtensions`** - Generic numeric operations for `INumber<T>` types
- **`EnumExtensions`** - Flag operations, descriptions, and enumeration utilities
- **`DictionaryExtensions`** - Dictionary operations and synchronization
- **`ListExtensions`** - List manipulation and utilities
- **`EnumerableExtensions`** - Collection operations and safe access methods
- **`DateTimeExtensions`** - DateTime range and percentage calculations
- **`DateOnlyExtensions`** - DateOnly-specific operations
- **`TimeOnlyExtensions`** - TimeOnly-specific operations
- **`ReflectionExtensions`** - Type discovery and attribute scanning
- **`AssemblyExtensions`** - Resource extraction and assembly utilities
- **`ComparableExtensions`** - Enhanced comparison operations
- **`ByteExtensions`** - Bit manipulation and byte operations
- **`GuidExtensions`** - GUID utilities and operations
- **`VersionExtensions`** - Version comparison and manipulation
- **`TaskExtensions`** - Task and async operation helpers
- **`TaskCompletionSourceExtensions`** - Enhanced TaskCompletionSource operations
- **`RandomExtensions`** - Additional random generation methods

### Utility Classes

- **`ComparableTools`** - Static utility methods for comparable types
- **`NumericRangeTools`** - Range generation for numeric types

---

## 🔧 Design Principles

- **Type Safety**: Generic constraints ensure compile-time safety and prevent runtime errors
- **Performance**: Optimized with modern .NET features, aggressive inlining, and efficient algorithms
- **Null Safety**: Explicit null handling with clear, predictable behavior
- **Consistency**: Uniform naming conventions and parameter patterns across all extensions
- **Extensibility**: Generic implementations work with user-defined types that implement required interfaces
- **Modern .NET**: Built for .NET 9 with latest C# language features and best practices

---

## 🧪 Development

- **Testing**: xUnit + FluentAssertions with 90%+ code coverage
- **Performance**: Benchmarked with BenchmarkDotNet for critical paths
- **Build**: `dotnet build`
- **Test**: `dotnet test`
- **Coverage**: `dotnet test --collect:"XPlat Code Coverage"`
- **Framework**: .NET 9.0

---

## 📄 License

MIT — see `LICENSE.md` for details.

---

*Designed for modern .NET applications requiring powerful, type-safe base type extensions.*
