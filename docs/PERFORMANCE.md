# SharpCompress Performance Guide

This guide helps you optimize SharpCompress for performance in various scenarios.

## API Selection Guide

### Archive API vs Reader API

Choose the right API based on your use case:

| Aspect | Archive API | Reader API |
|--------|------------|-----------|
| **Stream Type** | Seekable only | Non-seekable OK |
| **Memory Usage** | All entries in memory | One entry at a time |
| **Random Access** | ✓ Yes | ✗ No |
| **Best For** | Small-to-medium archives | Large or streaming data |
| **Performance** | Fast for random access | Better for large files |

### Archive API (Fast for Random Access)

```csharp
// Use when:
// - Archive fits in memory
// - You need random access to entries
// - Stream is seekable (file, MemoryStream)

using (var archive = ZipArchive.OpenArchive("archive.zip"))
{
    // Random access - all entries available
    var specific = archive.Entries.FirstOrDefault(e => e.Key == "file.txt");
    if (specific != null)
    {
        specific.WriteToFile(@"C:\output\file.txt");
    }
}
```

**Performance Characteristics:**
- ✓ Instant entry lookup
- ✓ Parallel extraction possible
- ✗ Entire archive in memory
- ✗ Can't process while downloading

### Reader API (Best for Large Files)

```csharp
// Use when:
// - Processing large archives (>100 MB)
// - Streaming from network/pipe
// - Memory is constrained
// - Forward-only processing is acceptable

using (var stream = File.OpenRead("large.zip"))
using (var reader = ReaderFactory.OpenReader(stream))
{
    while (reader.MoveToNextEntry())
    {
        // Process one entry at a time
        reader.WriteEntryToDirectory(@"C:\output");
    }
}
```

**Performance Characteristics:**
- ✓ Minimal memory footprint
- ✓ Works with non-seekable streams
- ✓ Can process while downloading
- ✗ Forward-only (no random access)
- ✗ Entry lookup requires iteration

### 7z solid folder decoded cache (Archive API)

Solid 7z folders normally require decompressing from the start of the folder to reach any entry. For Archive API random access (re-open same entry, backward order within a folder), SharpCompress can retain the most recently accessed folder's **decoded** bytes in a bounded in-memory buffer.

```csharp
var options = new ReaderOptions
{
    // Default: 128 MB. Set to 0 to disable decoded-folder caching.
    SolidFolderDecodedCacheMaxBytes = 128L * 1024 * 1024,
};
using var archive = SevenZipArchive.OpenArchive(stream, options);
using var entryStream = archive.Entries.First(e => !e.IsDirectory).OpenEntryStream();
```

When a folder's decompressed size fits within the cap, the first open decodes the whole folder once; subsequent opens in any order read slices from memory. Folders larger than the cap fall back to the streaming folder decoder (forward-only reuse within a live decode session).

The rewind ring buffer used by `ReaderFactory` for format detection is released after RAR detection via `FreezeAndReleaseBuffer`, so steady-state RAR Reader streaming does not pay ring copy overhead.

---

## Buffer Sizing

SharpCompress exposes **three distinct buffer knobs**. They are not interchangeable.

| Knob | Default | What it controls |
|------|---------|------------------|
| `Constants.BufferSize` | 81 920 bytes (80 KB) | Library-wide default for stream copy buffers; matches .NET `Stream.CopyTo` |
| `ReaderOptions.BufferSize` / `ExtractionOptions.BufferSize` / `WriterOptions.BufferSize` | `Constants.BufferSize` | Buffer passed to `CopyTo` / `CopyToAsync` during entry extraction or writing |
| `ReaderOptions.RewindableBufferSize` | `Constants.RewindableBufferSize` (81 920 bytes) when unset | Ring buffer for **non-seekable** streams during format auto-detection only |

Seekable inputs (`FileStream`, `MemoryStream`) do **not** allocate a rewind ring; they use native seek instead.

### Extraction copy buffers (`BufferSize`)

Use `ReaderOptions.BufferSize` or `ExtractionOptions.BufferSize` when calling high-level extract helpers, or pass `bufferSize` directly to `CopyTo` / `CopyToAsync`:

```csharp
// Sync: Reader API with larger extraction copy buffer
var options = new ReaderOptions { BufferSize = 262144 }; // 256 KB
using var reader = ReaderFactory.OpenReader(stream, options);
while (reader.MoveToNextEntry())
{
    if (!reader.Entry.IsDirectory)
    {
        reader.WriteEntryToDirectory(@"C:\output", new ExtractionOptions { BufferSize = 262144 });
    }
}

// Async: same knobs on async paths
var asyncOptions = new ReaderOptions { BufferSize = 262144 };
await using var reader = await ReaderFactory.OpenAsyncReader(stream, asyncOptions);
while (await reader.MoveToNextEntryAsync())
{
    if (!reader.Entry.IsDirectory)
    {
        await reader.WriteEntryToDirectoryAsync(
            @"C:\output",
            new ExtractionOptions { BufferSize = 262144 }
        );
    }
}

// Manual copy — bufferSize is always under your control
using (var entryStream = reader.OpenEntryStream())
using (var fileStream = File.Create(@"C:\output\file.txt"))
{
    entryStream.CopyTo(fileStream);                     // 81 920 B default
    entryStream.CopyTo(fileStream, bufferSize: 262144); // 256 KB
}

await using (var entryStream = await reader.OpenEntryStreamAsync())
await using (var fileStream = File.Create(@"C:\output\file.txt"))
{
    await entryStream.CopyToAsync(fileStream);              // 81 920 B default
    await entryStream.CopyToAsync(fileStream, 262144);      // 256 KB
}
```

| Scenario | Suggested copy buffer | Notes |
|----------|----------------------|-------|
| Default / local SSD | 81 920 B (default) | Matches BCL `CopyTo`; no config needed |
| Network or slow storage | 256 KB – 1 MB | Fewer syscalls; set via `BufferSize` or `CopyTo(..., bufferSize)` |
| Memory-constrained | 16 – 32 KB | Lower peak memory during parallel extraction |

### Rewind ring buffer (`RewindableBufferSize`)

When `ReaderFactory.OpenReader` / `OpenAsyncReader` receives a **non-seekable** stream, SharpCompress wraps it in a ring buffer so format detection can rewind and retry decoders. This buffer is **not** used for decompression throughput — only for header probing.

```csharp
// Default: Constants.RewindableBufferSize (81 920 bytes)
using var reader = ReaderFactory.OpenReader(networkStream);

// Raise for self-extracting archives or formats with large detection headers
var sfxOptions = new ReaderOptions
{
    LookForHeader = true,
    RewindableBufferSize = 1_048_576, // 1 MB — see ReaderOptions.ForSelfExtractingArchive
};
await using var asyncReader = await ReaderFactory.OpenAsyncReader(networkStream, sfxOptions);
```

**When to increase `RewindableBufferSize`:**

- Self-extracting RAR archives (512 KB – 1 MB+ stub before the archive)
- `"recording anchor"` / rewind overflow errors during format detection
- Tar combined formats: individual wrappers declare minimums (BZip2 blocks up to ~900 KB; Tar detection uses `TarWrapper.MaximumRewindBufferSize` automatically when opening `.tar.*` streams)

**Memory impact:** allocated only for non-seekable inputs. Seekable file paths pay zero ring-buffer cost.

---

## Streaming Large Files

### Non-Seekable Stream Patterns

For processing archives from downloads or pipes:

```csharp
// Download stream (non-seekable)
using (var httpStream = await httpClient.GetStreamAsync(url))
using (var reader = ReaderFactory.OpenReader(httpStream))
{
    // Process entries as they arrive
    while (reader.MoveToNextEntry())
    {
        if (!reader.Entry.IsDirectory)
        {
            reader.WriteEntryToDirectory(@"C:\output");
        }
    }
}
```

**Performance Tips:**
- Don't try to buffer the entire stream
- Process entries immediately
- Use async APIs for better responsiveness

### Download-Then-Extract vs Streaming

Choose based on your constraints:

| Approach | When to Use |
|----------|------------|
| **Download then extract** | Moderate size, need random access |
| **Stream during download** | Large files, bandwidth limited, memory constrained |

```csharp
// Download then extract (requires disk space)
var archivePath = await DownloadFile(url, @"C:\temp\archive.zip");
using (var archive = ZipArchive.OpenArchive(archivePath))
{
    archive.WriteToDirectory(@"C:\output");
}

// Stream during download (on-the-fly extraction)
using (var httpStream = await httpClient.GetStreamAsync(url))
using (var reader = ReaderFactory.OpenReader(httpStream))
{
    while (reader.MoveToNextEntry())
    {
        reader.WriteEntryToDirectory(@"C:\output");
    }
}
```

---

## Solid Archive Optimization

### Why Solid Archives Are Slow

Solid archives (Rar, 7Zip) group files together in a single compressed stream:

```
Solid Archive Layout:
[Header] [Compressed Stream] [Footer]
         ├─ File1 compressed data
         ├─ File2 compressed data
         ├─ File3 compressed data
         └─ File4 compressed data
```

Extracting File3 requires decompressing File1 and File2 first.

### Sequential vs Random Extraction

**Random Extraction (Slow):**
```csharp
using (var archive = RarArchive.OpenArchive("solid.rar"))
{
    foreach (var entry in archive.Entries)
    {
        entry.WriteToFile(@"C:\output\" + entry.Key);  // ✗ Slow!
        // Each entry triggers full decompression from start
    }
}
```

**Sequential Extraction (Fast):**
```csharp
using (var archive = RarArchive.OpenArchive("solid.rar"))
{
    // Method 1: Use WriteToDirectory (recommended)
    archive.WriteToDirectory(@"C:\output");
    
    // Method 2: Use ExtractAllEntries
    archive.ExtractAllEntries();
    
    // Method 3: Use Reader API (also sequential)
    using (var reader = RarReader.Open(File.OpenRead("solid.rar")))
    {
        while (reader.MoveToNextEntry())
        {
            reader.WriteEntryToDirectory(@"C:\output");
        }
    }
}
```

**Performance Impact:**
- Random extraction: O(n²) - very slow for many files
- Sequential extraction: O(n) - 10-100x faster

### Seeking Inside Compressed RAR Entries

HTTP range requests and media players often open an entry, read a slice at an offset, dispose, and repeat at another offset. Behavior depends on compression method:

| Entry type | Stream | Seek support | Cost to reach offset *O* |
|------------|--------|--------------|--------------------------|
| Stored (method 0), non-encrypted, non-solid | `StoredRarEntryStream` | Native `Seek` on volume data | O(1) — reposition only |
| Compressed or encrypted | `RarStream` | None (`CanSeek == false`) | O(offset) — full decode-and-discard from byte 0 |

**What happens on each `OpenEntryStream()` for compressed entries:**

1. A fresh `RarStream` and unpacker state are created (`RarArchiveEntry.OpenEntryStream`).
2. `Initialize()` calls `unpack.DoUnpack(...)` from the start of the compressed stream.
3. Skipping to an offset reads through the decompressor and discards output (no seek shortcut).
4. Disposing the stream tears down unpacker state; the next open starts over.

Stored entries on seekable volumes take the fast path in `StoredRarEntryStream.TryCreate` and seek directly to `DataStartPosition + offset`.

#### Measured costs (BenchmarkDotNet, Apple Silicon, Release)

Small entry (`Rar.rar`, ~60 KB compressed test entry) — `RarSeekPatternBenchmarks`:

| Pattern | Mean | Allocated/op |
|---------|------|--------------|
| Compressed: open → read 1 MB at 4 offsets → dispose | ~119 μs | ~133 KB |
| Stored (m0): open → `Seek` → read 1 MB at 4 offsets → dispose | ~11 μs | ~5 KB |

Large compressed entry (`Rar.issue1050.rar`, ~4.7 MB entry `Braid/766832.tr11dtp`) — `RarCompressedLargeSeekBenchmarks`:

| Pattern | Mean (Release, local) | Notes |
|---------|----------------------|-------|
| Read 1 MB at 0 / 25 / 50 / 75 % (4 opens) | ~134 ms | 75 % offset decodes ~3.5 MB then reads 1 MB |
| Above + backward re-read at 25 % | ~183 ms | Extra full decode from 0 to 25 % |

Costs scale roughly linearly with offset for compressed entries: reaching 50 % of a 50 MB entry decodes ~25 MB every time the stream is reopened. Set `SHARPCOMPRESS_SEEK_BENCH_RAR` to a local ~50 MB fixture to reproduce at full scale.

Run benchmarks:

```bash
dotnet run --project tests/SharpCompress.Performance/SharpCompress.Performance.csproj -c Release -- \
  --filter '*RarSeekPattern*' '*RarCompressedLargeSeek*'
```

#### Decoder checkpointing (design evaluation — not implemented)

Periodic snapshots of the LZSS window + range-coder state could bound backward-seek cost after the first pass, but:

- **Memory:** RAR5 dictionary is up to 64 MB per checkpoint; RAR4 up to 4 MB. Fixed-interval checkpoints multiply this cost.
- **CPU:** Snapshot/restore adds overhead on every forward read even when seeks never occur.
- **Complexity:** Unpack V1/V2017, solid streams, encryption, and multi-volume parts all need separate checkpoint paths.

**Recommendation:** defer implementation. If added later, use an **on-demand** model: snapshot only when the first backward seek is detected, keep one checkpoint per open entry stream, and document the memory trade-off. Track as a follow-up feature issue rather than a default code path.

For range-serve workloads today: prefer **stored (m0)** RAR entries when authoring archives; for compressed entries, cache one open stream per entry or extract sequentially.

### Best Practices for Solid Archives

1. **Always extract sequentially** when possible
2. **Use Reader API** for large solid archives
3. **Process entries in order** from the archive
4. **Consider using 7Zip command-line** for scripted extractions

---

## Compression Level Trade-offs

### Deflate/GZip Levels

```csharp
// Level 1 = Fastest, largest size
// Level 6 = Default (balanced)
// Level 9 = Slowest, best compression

// Write with different compression levels
using (var archive = ZipArchive.CreateArchive())
{
    archive.AddAllFromDirectory(@"D:\data");
    
    // Fast compression (level 1)
    archive.SaveTo("fast.zip", new WriterOptions(CompressionType.Deflate)
    {
        CompressionLevel = 1
    });
    
    // Default compression (level 6)
    archive.SaveTo("default.zip", CompressionType.Deflate);
    
    // Best compression (level 9)
    archive.SaveTo("best.zip", new WriterOptions(CompressionType.Deflate)
    {
        CompressionLevel = 9
    });
}
```

**Speed vs Size:**
| Level | Speed | Size | Use Case |
|-------|-------|------|----------|
| 1 | 10x | 90% | Network, streaming |
| 6 | 1x | 75% | Default (good balance) |
| 9 | 0.1x | 65% | Archival, static storage |

### BZip2 Block Size

```csharp
// BZip2 block size affects memory and compression
// 100K to 900K (default 900K)

// Smaller block size = lower memory, faster
// Larger block size = better compression, slower

using (var archive = TarArchive.CreateArchive())
{
    archive.AddAllFromDirectory(@"D:\data");
    
    // These are preset in WriterOptions via CompressionLevel
    archive.SaveTo("archive.tar.bz2", CompressionType.BZip2);
}
```

### LZMA Settings

LZMA compression is very powerful but memory-intensive:

```csharp
// LZMA (7Zip, .tar.lzma):
// - Dictionary size: 16 KB to 1 GB (default 32 MB)
// - Faster preset: smaller dictionary
// - Better compression: larger dictionary

// Preset via CompressionType
using (var archive = TarArchive.CreateArchive())
{
    archive.AddAllFromDirectory(@"D:\data");
    archive.SaveTo("archive.tar.xz", CompressionType.LZMA);  // Default settings
}
```

---

## Async Performance

### When Async Helps

Async is beneficial when:
- **Long I/O operations** (network, slow disks)
- **UI responsiveness** needed (Windows Forms, WPF, Blazor)
- **Server applications** (ASP.NET, multiple concurrent operations)

```csharp
// Async extraction (non-blocking)
using (var archive = ZipArchive.OpenArchive("archive.zip"))
{
    await archive.WriteToDirectoryAsync(
        @"C:\output",
        cancellationToken: cancellationToken
    );
}
// Thread can handle other work while I/O happens
```

### When Async Doesn't Help

Async doesn't improve performance for:
- **CPU-bound operations** (already fast)
- **Local SSD I/O** (I/O is fast enough)
- **Single-threaded scenarios** (no parallelism benefit)

```csharp
// Sync extraction (simpler, same performance on fast I/O)
using (var archive = ZipArchive.OpenArchive("archive.zip"))
{
    archive.WriteToDirectory(@"C:\output");
}
// Simple and fast - no async needed
```

### Cancellation Pattern

```csharp
var cts = new CancellationTokenSource();

// Cancel after 5 minutes
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    using (var archive = ZipArchive.OpenArchive("archive.zip"))
    {
        await archive.WriteToDirectoryAsync(
            @"C:\output",
            cancellationToken: cts.Token
        );
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Extraction cancelled");
    // Clean up partial extraction if needed
}
```

---

## Practical Performance Tips

### 1. Choose the Right API

| Scenario | API | Why |
|----------|-----|-----|
| Small archives | Archive | Faster random access |
| Large archives | Reader | Lower memory |
| Streaming | Reader | Works on non-seekable streams |
| Download streams | Reader | Async extraction while downloading |

### 2. Batch Operations

```csharp
// ✗ Slow - opens each archive separately
foreach (var file in files)
{
    using (var archive = ZipArchive.OpenArchive("archive.zip"))
    {
        archive.WriteToDirectory(@"C:\output");
    }
}

// ✓ Better - process multiple entries at once
using (var archive = ZipArchive.OpenArchive("archive.zip"))
{
    archive.WriteToDirectory(@"C:\output");
}
```

### 3. Profile Your Code

```csharp
var sw = Stopwatch.StartNew();
using (var archive = ZipArchive.OpenArchive("large.zip"))
{
    archive.WriteToDirectory(@"C:\output");
}
sw.Stop();

Console.WriteLine($"Extraction took {sw.ElapsedMilliseconds}ms");

// Measure memory before/after
var beforeMem = GC.GetTotalMemory(true);
// ... do work ...
var afterMem = GC.GetTotalMemory(true);
Console.WriteLine($"Memory used: {(afterMem - beforeMem) / 1024 / 1024}MB");
```

---

## Troubleshooting Performance

### Extraction is Slow

1. **Check if solid archive** → Use sequential extraction
2. **Check API** → Reader API might be faster for large files
3. **Check compression level** → Higher levels are slower to decompress
4. **Check I/O** → Network drives are much slower than SSD
5. **Check copy buffer size** — raise `ExtractionOptions.BufferSize` or `CopyTo(..., bufferSize)` for slow network I/O; raise `RewindableBufferSize` only for non-seekable format-detection failures

### High Memory Usage

1. **Use Reader API** instead of Archive API
2. **Process entries immediately** rather than buffering
3. **Reduce compression level** if writing
4. **Check for memory leaks** in your code

### CPU Usage at 100%

1. **Normal for compression** - especially with high compression levels
2. **Consider lower level** for faster processing
3. **Reduce parallelism** if processing multiple archives
4. **Check if awaiting properly** in async code

---

## Related Documentation

- [USAGE.md](USAGE.md) - Usage examples with performance considerations
- [FORMATS.md](FORMATS.md) - Format-specific performance notes
