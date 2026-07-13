using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// HTTP-range-style patterns against a large compressed (non-stored) RAR entry.
/// Uses <c>Rar.issue1050.rar</c> (~4.7 MB entry) by default; set
/// <c>SHARPCOMPRESS_SEEK_BENCH_RAR</c> to a path with a ~50 MB compressed entry for full-scale runs.
/// Each offset read re-opens the entry stream, so later offsets pay decode-and-discard cost from byte 0.
/// </summary>
[MemoryDiagnoser]
public class RarCompressedLargeSeekBenchmarks : ArchiveBenchmarkBase
{
    private const int ChunkSize = 1024 * 1024;
    private const string DefaultArchive = "Rar.issue1050.rar";

    private byte[] _archiveBytes = null!;
    private long _entrySize;
    private long[] _offsets = null!;
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        var archivePath =
            Environment.GetEnvironmentVariable("SHARPCOMPRESS_SEEK_BENCH_RAR")
            ?? GetArchivePath(DefaultArchive);

        if (!File.Exists(archivePath))
        {
            throw new InvalidOperationException(
                $"Seek benchmark archive not found: {archivePath}. "
                    + "Set SHARPCOMPRESS_SEEK_BENCH_RAR or add "
                    + DefaultArchive
                    + "."
            );
        }

        _archiveBytes = File.ReadAllBytes(archivePath);
        using var stream = new MemoryStream(_archiveBytes);
        using var archive = RarArchive.OpenArchive(stream);
        var entry = archive.Entries.Where(e => !e.IsDirectory && e.Size > 0).MaxBy(e => e.Size)!;

        _entrySize = entry.Size;
        _offsets = [0, _entrySize / 4, _entrySize / 2, (_entrySize * 3) / 4];
        _buffer = new byte[ChunkSize];
    }

    [Benchmark(Description = "Rar compressed large: open→read 1MB at 0/25/50/75%→dispose")]
    public long ForwardSeekStorm() => SeekStorm(includeBackward: false);

    [Benchmark(Description = "Rar compressed large: forward storm + backward re-read at 25%")]
    public long ForwardAndBackwardSeekStorm() => SeekStorm(includeBackward: true);

    private long SeekStorm(bool includeBackward)
    {
        long total = 0;
        using var stream = new MemoryStream(_archiveBytes);
        using var archive = RarArchive.OpenArchive(stream);
        var entry = archive.Entries.Where(e => !e.IsDirectory && e.Size > 0).MaxBy(e => e.Size)!;

        foreach (var offset in _offsets)
        {
            total += ReadAtOffset(entry, offset);
        }

        if (includeBackward)
        {
            total += ReadAtOffset(entry, _entrySize / 4);
        }

        return total;
    }

    private long ReadAtOffset(IArchiveEntry entry, long offset)
    {
        if (offset >= _entrySize)
        {
            return 0;
        }

        using var entryStream = entry.OpenEntryStream();
        Skip(entryStream, offset);
        var toRead = (int)Math.Min(ChunkSize, _entrySize - offset);
        return entryStream.Read(_buffer, 0, toRead);
    }

    private static void Skip(Stream stream, long count)
    {
        var remaining = count;
        var buffer = new byte[81920];
        while (remaining > 0)
        {
            var toRead = (int)Math.Min(buffer.Length, remaining);
            var read = stream.Read(buffer, 0, toRead);
            if (read == 0)
            {
                break;
            }

            remaining -= read;
        }
    }
}
