# Contributing to NzbDav.SharpCompress

Thank you for contributing. Please keep changes focused and include regression
tests for archive, reader/writer, stream ownership, cancellation, and format
behavior when applicable.

## Prerequisites

- The .NET SDK selected by `global.json`
- Git

Restore, build, run tests, and validate the package before opening a pull
request:

```bash
dotnet tool restore
dotnet restore SharpCompress.slnx --locked-mode
dotnet build SharpCompress.slnx --configuration Release --no-restore
dotnet csharpier check .
dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj --configuration Release --no-build
dotnet pack src/SharpCompress/SharpCompress.csproj --configuration Release --no-build --output artifacts
```

## Pull requests

1. Open an issue first for large API or architectural changes.
2. Preserve public API compatibility unless a breaking change is intentional
   and documented.
3. Update documentation when behavior or public APIs change.
4. Add or update tests that fail without the change.
5. Complete the pull request template and ensure all required checks pass.

Use scoped Conventional Commit subjects such as `fix(zip):`, `feat(rar):`,
`docs:`, or `chore(ci):`. Release Please uses commit history to prepare release
notes and determine semantic versions.

## Code review checklist

- **Sync/async twins:** If you touched a file that has a `.Async.cs` twin (for
  example `Foo.cs` and `Foo.Async.cs`), show in the PR that the twin was updated
  in the same change, or explain why no twin change was required. This prevents the
  sync and async code paths from drifting apart. See the
  [sync/async parity strategy](docs/ARCHITECTURE.md#syncasync-parity-strategy) for
  the pattern to follow (shared span-based IO-free cores; structural parity where
  buffers can't be shared; no faked async).

## Releases

Release Please maintains a release pull request. Merging that pull request
updates `.release-please-manifest.json` and the `<Version>` in
`src/SharpCompress/SharpCompress.csproj`, then creates an immutable `vX.Y.Z` tag
and a GitHub release. The release event builds, tests, validates, and publishes
the package to NuGet.org as `NzbDav.SharpCompress`. Maintainers should not
manually move release tags.
