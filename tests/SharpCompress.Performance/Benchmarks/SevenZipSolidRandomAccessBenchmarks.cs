using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives.SevenZip;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// Solid 7z Archive API random/sequential entry open baseline (finding 2.6).
/// </summary>
[MemoryDiagnoser]
public class SevenZipSolidRandomAccessBenchmarks : ArchiveBenchmarkBase
{
    private byte[] _solidBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solidBytes = File.ReadAllBytes(GetArchivePath("7Zip.solid.7z"));
    }

    [Benchmark(Description = "7z solid: open each entry via Archive API")]
    public void SevenZipSolidOpenEachEntry()
    {
        using var stream = new MemoryStream(_solidBytes);
        using var archive = SevenZipArchive.OpenArchive(stream);
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            using var entryStream = entry.OpenEntryStream();
            entryStream.CopyTo(Stream.Null);
        }
    }
}
