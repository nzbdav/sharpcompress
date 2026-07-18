---
description: 'Guidelines for building SharpCompress - A C# compression library'
applyTo: '**/*.cs'
---

# SharpCompress Development

## Purpose and stack

- This repository is the [nzbdav](https://github.com/nzbdav) fork of
  [adamhathcock/sharpcompress](https://github.com/adamhathcock/sharpcompress),
  maintained at [nzbdav/sharpcompress](https://github.com/nzbdav/sharpcompress).
- NuGet package id is `NzbDav.SharpCompress`. Assembly name and namespaces remain
  `SharpCompress` so consumers keep `using SharpCompress.*`.
- The library targets **.NET 10 only** (`net10.0`).
- In the nzbdav stack, SharpCompress is used for archive extraction (especially
  RAR and 7z) inside [nzbdav](https://github.com/nzbdav/nzbdav). Sibling libraries
  in the Usenet streaming dependency tree include UsenetSharp, RapidYencSharp, and
  rapidyenc. See the
  [nzbdav stack release announcement](https://github.com/nzbdav/nzbdav/blob/main/docs/release-announcement.md)
  for coordinated release context.
- SharpCompress is a pure C# compression library supporting multiple archive
  formats (Zip, Tar, GZip, BZip2, 7Zip, Rar, LZip, XZ, ZStandard, Arc, Arj, Ace,
  LZW). It provides both seekable Archive APIs and forward-only Reader/Writer APIs.
- Release Please versions the package; GitHub Actions builds and publishes releases.

## C# Instructions
- Use language features supported by the current project toolchain (`LangVersion=latest`) and existing codebase patterns.
- Add comments for non-obvious logic and important design decisions; avoid redundant comments.
- Follow the existing code style and patterns in the codebase.

## General Instructions
- **Do not commit or stage changes unless the user explicitly asks for it.**
- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.
- Preserve backward compatibility when making changes to public APIs.

### Workspace Hygiene
- Do not edit generated or machine-local files unless required for the task (for example: `bin/`, `obj/`, `*.csproj.user`).
- Avoid broad formatting-only diffs in unrelated files.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Code Formatting

**Copilot agents: You MUST run the `format` task after making code changes to ensure consistency.**

- Use CSharpier for code formatting to ensure consistent style across the project
- CSharpier is configured as a local tool in `.config/dotnet-tools.json`

### Commands

1. **Restore tools** (first time only):
   ```bash
   dotnet tool restore
   ```

2. **Check if files are formatted correctly** (doesn't modify files):
   ```bash
   dotnet csharpier check .
   ```
   - Exit code 0: All files are properly formatted
   - Exit code 1: Some files need formatting (will show which files and differences)

3. **Format files** (modifies files):
   ```bash
   dotnet csharpier format .
   ```
   - Formats all files in the project to match CSharpier style
   - Run from project root directory

4. **Configure your IDE** to format on save using CSharpier for the best experience

### Additional Notes
- The project also uses `.editorconfig` for editor settings (indentation, encoding, etc.)
- Let CSharpier handle code style while `.editorconfig` handles editor behavior
- Always run `dotnet csharpier check .` before committing to verify formatting

## Development workflow

```bash
dotnet tool restore
dotnet restore SharpCompress.slnx --locked-mode
dotnet build SharpCompress.slnx --configuration Release --no-restore
dotnet csharpier check .
dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj --configuration Release --no-build
dotnet pack src/SharpCompress/SharpCompress.csproj --configuration Release --no-build --output artifacts
```

## Project Setup and Structure

- Target framework: .NET 10 (`net10.0`)
- Main library is in `src/SharpCompress/`
- Tests are in `tests/SharpCompress.Test/`
- Performance tests are in `tests/SharpCompress.Performance/`
- Test archives are in `tests/TestArchives/`
- Build project is in `build/` (local helpers such as format and benchmarks; CI uses `dotnet` CLI directly)
- Use `dotnet build` to build the solution
- Use `dotnet test` to run tests
- Solution file: `SharpCompress.slnx`

### Directory Structure
```
src/SharpCompress/
  ├── Archives/        # IArchive implementations (Zip, Tar, Rar, 7Zip, GZip)
  ├── Readers/         # IReader implementations (forward-only)
  ├── Writers/         # IWriter implementations (forward-only)
  ├── Compressors/     # Low-level compression streams (BZip2, Deflate, LZMA, etc.)
  ├── Factories/       # Format detection and factory pattern
  ├── Common/          # Shared types (ArchiveType, Entry, Options)
  ├── Crypto/          # Encryption implementations
  └── IO/              # Stream utilities and wrappers

tests/SharpCompress.Test/
  ├── Zip/, Tar/, Rar/, SevenZip/, GZip/, BZip2/  # Format-specific tests
  ├── TestBase.cs      # Base test class with helper methods

tests/
  ├── SharpCompress.Test/         # Unit/integration tests
  ├── SharpCompress.Performance/  # Benchmark tests
  └── TestArchives/               # Test data archives
```

### Factory Pattern
Factory implementations can implement one or more interfaces (`IArchiveFactory`, `IReaderFactory`, `IWriterFactory`) depending on format capabilities:
- `ArchiveFactory.OpenArchive()` - Opens archive API objects from seekable streams/files
- `ArchiveFactory.OpenAsyncArchive()` - Opens async archive API objects for async archive use cases
- `ReaderFactory.OpenReader()` - Auto-detects and opens forward-only readers
- `ReaderFactory.OpenAsyncReader()` - Auto-detects and opens forward-only async readers
- `WriterFactory.OpenWriter()` - Creates a writer for a specified `ArchiveType`
- `WriterFactory.OpenAsyncWriter()` - Creates an async writer for async write scenarios
- Factories located in: `src/SharpCompress/Factories/`

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

## SharpCompress-Specific Guidelines

### Supported Formats
SharpCompress supports multiple archive and compression formats:
- **Archive Formats**: Zip, Tar, 7Zip, Rar (read-only), Ace (read-only), Arc (read-only), Arj (read-only), LZW (read-only)
- **Compression**: DEFLATE, BZip2, LZMA/LZMA2, PPMd, ZStandard, LZip, XZ (decompress only), Deflate64 (decompress only), legacy Zip/Arc/Arj/Ace methods (read-only as applicable)
- **Combined Formats**: Tar.GZip, Tar.BZip2, Tar.LZip, Tar.XZ (decompress only), Tar.ZStandard (decompress only), Tar.LZW (decompress only)
- **ZIP ZStandard**: ZIP supports ZStandard reading and writing; Tar.ZStandard is decompress-only.
- See [docs/FORMATS.md](docs/FORMATS.md) for complete format support matrix

### Stream Handling Rules
- **Disposal semantics**: The default `ReaderOptions.LeaveStreamOpen` value is `false`, but effective stream ownership depends on which API overload you call
  - File-based overloads (e.g., `OpenArchive(string filePath)`) open the file internally and own that stream, so it is closed by default with the archive/reader
    - Do **not** rely on a specific `ReaderOptions` preset being used internally; some implementations may use `ReaderOptions.ForFilePath`, while others may use default `ReaderOptions` with the same ownership semantics
  - Several high-level overloads that accept a caller-provided `Stream` use external-stream semantics by default (for example, `ReaderFactory.OpenReader(Stream)` / `ArchiveFactory.OpenArchive(Stream)`), so the caller's stream is typically left open unless you opt into different ownership behavior
    - Do **not** assume every stream-based overload behaves identically; some APIs require you to pass stream ownership options explicitly
- **For caller-provided streams**: When the overload accepts `ReaderOptions`, pass `ReaderOptions.ForExternalStream` or use `ReaderOptions` with `LeaveStreamOpen = true` whenever the caller must retain ownership of the stream
  - Example: `var options = new ReaderOptions { LeaveStreamOpen = true };`
  - Or: `var options = ReaderOptions.ForExternalStream;`
- **For file paths**: SharpCompress manages the stream lifecycle for the internally opened file stream; no manual disposal is needed beyond the archive/reader itself
- Use `NonDisposingStream` wrapper when working with compression streams directly to prevent disposal
- Always dispose of readers, writers, and archives in `using` / `await using` blocks
- For forward-only operations, use Reader/Writer APIs; for random access, use Archive APIs

### Async/Await Patterns
- All I/O operations support async/await with `CancellationToken`
- Async methods follow the naming convention: `MethodNameAsync`
- For async archive scenarios, prefer `ArchiveFactory.OpenAsyncArchive(...)` over sync `OpenArchive(...)`.
- For async forward-only read scenarios, prefer `ReaderFactory.OpenAsyncReader(...)` over sync `OpenReader(...)`.
- For async write scenarios, prefer `WriterFactory.OpenAsyncWriter(...)` over sync `OpenWriter(...)`.
- Key async methods:
  - `WriteEntryToAsync` - Extract entry asynchronously
  - `WriteAllToDirectoryAsync` - Extract all entries asynchronously
  - `WriteAsync` - Write entry asynchronously
  - `WriteAllAsync` - Write directory asynchronously
  - `OpenEntryStreamAsync` - Open entry stream asynchronously
- Always provide `CancellationToken` parameter in async methods

### Archive APIs vs Reader/Writer APIs
- **Archive API**: Use for random access with seekable streams (e.g., `ZipArchive`, `TarArchive`)
- **Reader API**: Use for forward-only reading on non-seekable streams (e.g., `ZipReader`, `TarReader`)
- **Writer API**: Use for forward-only writing on streams (e.g., `ZipWriter`, `TarWriter`)
- 7Zip only supports Archive API due to format limitations

### Tar-Specific Considerations
- Tar format requires file size in the header
- If no size is specified to TarWriter and the stream is not seekable, an exception will be thrown
- Tar combined with compression is supported for reading with GZip, BZip2, LZip, XZ, ZStandard, and LZW
- Tar writing supports uncompressed Tar, GZip, BZip2, and LZip wrappers

### Zip-Specific Considerations
- Supports Zip64 for large files (seekable streams only)
- Supports PKWare and WinZip AES encryption
- Multiple compression methods: None, Shrink, Reduce, Implode, DEFLATE, Deflate64, BZip2, LZMA, PPMd
- Encrypted LZMA is not supported

### Performance Considerations
- For large files, use Reader/Writer APIs with non-seekable streams to avoid loading entire file in memory
- Leverage async I/O for better scalability
- Consider compression level trade-offs (speed vs. size)
- Use appropriate buffer sizes for stream operations

## Repository and release
- `README.md` is user-facing documentation and is packed into the NuGet package.
- `.github/workflows/ci.yml` is the required pull-request quality gate.
- `release-please-config.json` updates the version in `src/SharpCompress/SharpCompress.csproj`.
- Release artifacts must be built from the release tag only after restore, build, tests, and package validation succeed.
- Published package id: `NzbDav.SharpCompress`.

## Commit convention
- Use scoped Conventional Commits: `feat(scope):`, `fix(scope):`, or `chore(scope):`.
- Choose a concise scope such as `zip`, `tar`, `rar`, `sevenzip`, `stream`, `ci`, `deps`, or `docs`.
- Release Please uses commit types for release notes and versions: `feat` triggers a minor release, `fix` triggers a patch release, and `chore` does not trigger a release.
- Mark breaking changes with `!` (for example, `feat(rar)!:`) and include a `BREAKING CHANGE:` footer.
- Keep unrelated changes in separate commits so each release-note entry describes one coherent change.

## Testing

- Always include test cases for critical paths of the application.
- Test with multiple archive formats when making changes to core functionality.
- Include tests for both Archive and Reader/Writer APIs when applicable.
- Test async operations with cancellation tokens.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Use test archives from `tests/TestArchives` directory for consistency.
- Test stream disposal and `LeaveStreamOpen` behavior.
- Test edge cases: empty archives, large files, corrupted archives, encrypted archives.

### Validation Expectations
- Run targeted tests for the changed area first.
- Run `dotnet csharpier format .` after code edits.
- Run `dotnet csharpier check .` before handing off changes.

### Test Organization
- Base class: `TestBase` - Provides `TEST_ARCHIVES_PATH`, `SCRATCH_FILES_PATH`, temp directory management
- Framework: xUnit with AwesomeAssertions
- Test archives: `tests/TestArchives/` - Use existing archives, don't create new ones unnecessarily
- Match naming style of nearby test files

### Public API Change Checklist
- Preserve existing public method signatures and behavior when possible.
- If a breaking change is unavoidable, document it and provide a migration path.
- Add or update tests that cover backward compatibility expectations.
- Avoid exposing public `init` setters, positional records, `required` members, or other metadata that forces consumers onto newer C# language versions; validate older-consumer compatibility with tests when changing exported APIs.

### Public API Documentation Checklist
- When adding, removing, renaming, or changing public APIs, update the public docs in the same change.
- Check `docs/API.md` for new factory methods, options, interfaces, extension methods, enums, archive/reader/writer APIs, compression provider APIs, and examples.
- Check `docs/USAGE.md` when the API change affects recommended usage patterns or requires a new example.
- Check `docs/FORMATS.md` when the API change affects supported archive formats, compression methods, reader/archive/writer availability, or detection behavior.
- Check `README.md` when the change affects the top-level support summary, target frameworks, major capabilities, or user-facing feature list.
- For public API changes, verify examples compile conceptually against the actual public signatures and avoid documenting internal-only types.

### Stream Ownership and Position Checklist
- Verify `LeaveStreamOpen` behavior for externally owned streams.
- Validate behavior for both seekable and non-seekable streams.
- Ensure stream position assumptions are explicit and tested.

## RAR header-read exception contract

`RarHeaderFactory.ReadHeaders` / `ReadHeadersAsync` surface parse failures as
`SharpCompress.Common.Rar.RarHeaderReadException` (see issue #119):

| `Truncated` | Meaning | Typical causes |
|-------------|---------|----------------|
| `true` | Stream ended before a complete header could be read, or seek past end while skipping packed data | Signature-scan EOF, mid-header EOF, deferred data-skip past `Length` |
| `false` | Corrupt or unsupported header content | Header CRC mismatch, unknown header code, signature not found after a full scan, pre-RAR4 format |

Prefer matching `RarHeaderReadException` over raw BCL types
(`ArgumentOutOfRangeException`, `EndOfStreamException`,
`IncompleteArchiveException`). Successful parses are unchanged.

Mid-enumeration EOF while filling the next header block ends the enumeration
gracefully (archives may omit EndArchive). Signature-scan EOF and seek-past-end
during packed-data skip still throw with `Truncated = true`.

## Common Pitfalls

1. **Don't mix Archive and Reader APIs** - Archive needs seekable stream, Reader doesn't
2. **Don't mix sync and async open paths** - For async workflows use `OpenAsyncArchive`/`OpenAsyncReader`/`OpenAsyncWriter`, not `OpenArchive`/`OpenReader`/`OpenWriter`
3. **Solid archives (Rar, 7Zip)** - Use `ExtractAllEntries()` for best performance, not individual entry extraction
4. **Stream disposal** - Always set `LeaveStreamOpen` explicitly when needed (default is to close)
5. **Tar + non-seekable stream** - Must provide file size or it will throw
6. **Format detection** - Use `ReaderFactory.OpenReader()` / `ReaderFactory.OpenAsyncReader()` for auto-detection, test with actual archive files
