using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives.Rar;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// HTTP-range-style open→read-at-offset→dispose patterns against small RAR entry streams.
/// Stored (m0) entries use real <see cref="StoredRarEntryStream"/> seek; compressed entries
/// re-open <see cref="RarStream"/> and decode-and-discard to the target offset.
/// See <see cref="RarCompressedLargeSeekBenchmarks"/> for large compressed entry costs.
/// </summary>
[MemoryDiagnoser]
public class RarSeekPatternBenchmarks : ArchiveBenchmarkBase
{
    private const int ChunkSize = 1024 * 1024;
    private static readonly long[] Offsets = [0, 64 * 1024, 256 * 1024, 512 * 1024];

    private byte[] _rarBytes = null!;
    private byte[] _rarNoneBytes = null!;
    private byte[] _rar5NoneBytes = null!;
    private byte[] _rar5NoneEncryptedBytes = null!;
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _rarBytes = File.ReadAllBytes(GetArchivePath("Rar.rar"));
        _rarNoneBytes = File.ReadAllBytes(GetArchivePath("Rar.none.rar"));
        _rar5NoneBytes = File.ReadAllBytes(GetArchivePath("Rar5.none.rar"));
        _rar5NoneEncryptedBytes = File.ReadAllBytes(GetArchivePath("Rar5.none.encrypted.rar"));
        _buffer = new byte[ChunkSize];
    }

    [Benchmark(Description = "Rar compressed: open→read 1MB at N offsets→dispose")]
    public long RarSeekStorm() => SeekStorm(_rarBytes, useSeek: false);

    [Benchmark(Description = "Rar stored (m0): open→Seek→read 1MB at N offsets→dispose")]
    public long RarStoredSeekStorm() => SeekStorm(_rarNoneBytes, useSeek: true);

    [Benchmark(Description = "Rar5 stored (m0): open→Seek→read 1MB at N offsets→dispose")]
    public long Rar5StoredSeekStorm() => SeekStorm(_rar5NoneBytes, useSeek: true);

    [Benchmark(Description = "Rar5 encrypted stored (m0): open→Seek→read 1MB at N offsets→dispose")]
    public long Rar5EncryptedStoredSeekStorm() =>
        SeekEncryptedStorm(_rar5NoneEncryptedBytes, useSeek: true);

    private long SeekStorm(byte[] archiveBytes, bool useSeek)
    {
        long total = 0;
        using var stream = new MemoryStream(archiveBytes);
        using var archive = RarArchive.OpenArchive(stream);
        var entry = archive.Entries.First(e => !e.IsDirectory && e.Size > 0);

        foreach (var offset in Offsets)
        {
            if (offset >= entry.Size)
            {
                continue;
            }

            using var entryStream = entry.OpenEntryStream();
            if (useSeek && entryStream.CanSeek)
            {
                entryStream.Seek(offset, SeekOrigin.Begin);
            }
            else
            {
                Skip(entryStream, offset);
            }

            var toRead = (int)Math.Min(ChunkSize, entry.Size - offset);
            var read = entryStream.Read(_buffer, 0, toRead);
            total += read;
        }

        return total;
    }

    private long SeekEncryptedStorm(byte[] archiveBytes, bool useSeek)
    {
        long total = 0;
        using var stream = new MemoryStream(archiveBytes);
        using var archive = RarArchive.OpenArchive(
            stream,
            new SharpCompress.Readers.ReaderOptions { Password = "test" }
        );
        var entry = archive.Entries.First(e => !e.IsDirectory && e.Size > 0);

        foreach (var offset in Offsets)
        {
            if (offset >= entry.Size)
            {
                continue;
            }

            using var entryStream = entry.OpenEntryStream();
            if (useSeek && entryStream.CanSeek)
            {
                entryStream.Seek(offset, SeekOrigin.Begin);
            }
            else
            {
                Skip(entryStream, offset);
            }

            var toRead = (int)Math.Min(ChunkSize, entry.Size - offset);
            var read = entryStream.Read(_buffer, 0, toRead);
            total += read;
        }

        return total;
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
