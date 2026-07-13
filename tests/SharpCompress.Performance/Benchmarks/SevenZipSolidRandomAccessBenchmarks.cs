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
    private string[] _entryKeys = null!;

    [GlobalSetup]
    public void Setup()
    {
        _solidBytes = File.ReadAllBytes(GetArchivePath("7Zip.solid.7z"));
        using var archive = SevenZipArchive.OpenArchive(new MemoryStream(_solidBytes));
        _entryKeys = archive.Entries.Where(e => !e.IsDirectory).Select(e => e.Key!).ToArray();
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

    [Benchmark(Description = "7z solid: re-open same entry twice")]
    public void SevenZipSolidReOpenSameEntry()
    {
        using var stream = new MemoryStream(_solidBytes);
        using var archive = SevenZipArchive.OpenArchive(stream);
        var key = _entryKeys[^1];
        for (var i = 0; i < 2; i++)
        {
            var entry = archive.Entries.First(e => e.Key == key);
            using var entryStream = entry.OpenEntryStream();
            entryStream.CopyTo(Stream.Null);
        }
    }

    [Benchmark(Description = "7z solid: open entries in reverse folder order")]
    public void SevenZipSolidOpenEntriesReverse()
    {
        using var stream = new MemoryStream(_solidBytes);
        using var archive = SevenZipArchive.OpenArchive(stream);
        foreach (var key in _entryKeys.AsEnumerable().Reverse())
        {
            var entry = archive.Entries.First(e => e.Key == key);
            using var entryStream = entry.OpenEntryStream();
            entryStream.CopyTo(Stream.Null);
        }
    }

    [Benchmark(Description = "7z solid: open entry N then entry N-1")]
    public void SevenZipSolidOpenAdjacentEntriesBackward()
    {
        using var stream = new MemoryStream(_solidBytes);
        using var archive = SevenZipArchive.OpenArchive(stream);
        var last = _entryKeys[^1];
        var previous = _entryKeys[^2];

        var lastEntry = archive.Entries.First(e => e.Key == last);
        using (var entryStream = lastEntry.OpenEntryStream())
        {
            entryStream.CopyTo(Stream.Null);
        }

        var previousEntry = archive.Entries.First(e => e.Key == previous);
        using (var entryStream = previousEntry.OpenEntryStream())
        {
            entryStream.CopyTo(Stream.Null);
        }
    }
}
