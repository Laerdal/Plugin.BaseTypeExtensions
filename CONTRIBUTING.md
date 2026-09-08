# Contributing

## Branching

Work off a short-lived feature branch cut from `main`. Open a PR early and mark it as a Draft
if it's still in progress.

## Commit convention

This repo uses [Conventional Commits](https://www.conventionalcommits.org/): `type(scope): subject`,
imperative mood, <= 72 characters. Common types: `feat`, `fix`, `docs`, `style`, `refactor`,
`perf`, `test`, `chore`. Scope is optional but recommended (e.g. the extension category touched).

## Pull requests

Fill out the PR template. Keep each PR scoped to one logical change. CI must pass before merge.

## Code style

Follow this repo's `.editorconfig`. Prefer clarity over cleverness; avoid unnecessary abstraction
— this is a firm preference, not a suggestion. Keep each extension category in its own
`*Extensions.cs` file named after the type it extends (e.g. `StringExtensions.cs`,
`DateTimeExtensions.cs`) — don't mix categories in one file. See
`.github/copilot-instructions.md` for the existing conventions.

## Testing

All new functionality needs xUnit tests in `Plugin.BaseTypeExtensions.Tests`, using
FluentAssertions for assertions. Test both success and failure paths and boundary conditions
(null, empty, min/max values).

## Documentation

Public APIs need XML doc comments (`<summary>`, `<param>`, `<returns>`, `<exception>`). If you
add a new extension category, update the README's feature list too.
