using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives.Rar;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// HTTP-range-style open→read-at-offset→dispose patterns against RAR entry streams.
/// Until stored-entry seek (2.1) lands, seeks may drain; this is still a useful baseline.
/// </summary>
[MemoryDiagnoser]
public class RarSeekPatternBenchmarks : ArchiveBenchmarkBase
{
    private const int ChunkSize = 1024 * 1024;
    private static readonly long[] Offsets = [0, 64 * 1024, 256 * 1024, 512 * 1024];

    private byte[] _rarBytes = null!;
    private byte[] _buffer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _rarBytes = File.ReadAllBytes(GetArchivePath("Rar.rar"));
        _buffer = new byte[ChunkSize];
    }

    [Benchmark(Description = "Rar: open→read 1MB at N offsets→dispose")]
    public long RarSeekStorm()
    {
        long total = 0;
        using var stream = new MemoryStream(_rarBytes);
        using var archive = RarArchive.OpenArchive(stream);
        var entry = archive.Entries.First(e => !e.IsDirectory);

        foreach (var offset in Offsets)
        {
            if (offset >= entry.Size)
            {
                continue;
            }

            using var entryStream = entry.OpenEntryStream();
            Skip(entryStream, offset);
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
