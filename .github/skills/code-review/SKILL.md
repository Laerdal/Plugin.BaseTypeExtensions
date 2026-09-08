---
name: code-review
description: Use for reviewing pull requests on Plugin.BaseTypeExtensions — checks file-per-category organization, generic numeric constraints, and null/boundary safety on new extension methods.
---

# Code Review — Plugin.BaseTypeExtensions

## Checklist for new or changed extension methods

- [ ] **One file per extended type/category**: a new extension method belongs in the
      `*Extensions.cs` file named after the type it extends (e.g. a new `DateTime` helper goes in
      `DateTimeExtensions.cs`, not bundled into an unrelated file). A PR mixing categories into
      one file, or adding a same-named method to two files, is a real finding.
- [ ] **Generic numeric code uses `INumber<T>`**: new numeric operations should use generic math
      constraints rather than adding per-type overloads (`int`, `double`, `float`, ...) — flag
      copy-pasted overloads as a sign the generic constraint was skipped.
- [ ] **Null/boundary safety**: string, collection, and comparable extensions handle `null`
      input explicitly (per this library's "null-safe" positioning) rather than throwing an
      unhandled `NullReferenceException`.
- [ ] **Standalone helpers stay standalone**: logic that doesn't read naturally as an extension
      method (e.g. clamping across a pair of bounds) belongs in `ComparableTools`/
      `NumericRangeTools` as static helpers — don't bolt it onto `ComparableExtensions`/
      `NumericExtensions` as an awkward extension method just to keep the "everything is an
      extension" pattern.
- [ ] **XML docs**: new public members have accurate `<summary>`/`<param>`/`<returns>`/
      `<exception>` docs.
- [ ] **README feature list**: a new extension category should be reflected there.

## What to flag as a real risk, not a nit

- A new extension method that duplicates behavior already available in `System.Linq` or the
  BCL without a clear reason (this library's value is filling *gaps*, not re-wrapping existing
  APIs).
- Breaking changes to existing extension method signatures — this is a published NuGet package
  consumed by other repos in this org (e.g. `Plugin.ExceptionListeners`), so a signature change
  is a semver-relevant, not a routine tweak.
