using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// Stored (m0) RAR streaming baselines for nzbdav-style sequential entry reads.
/// </summary>
[MemoryDiagnoser]
public class RarStoredStreamingBenchmarks : ArchiveBenchmarkBase
{
    private byte[] _rarNoneBytes = null!;
    private byte[] _rar5NoneBytes = null!;
    private byte[][] _multiVolumeBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _rarNoneBytes = File.ReadAllBytes(GetArchivePath("Rar.none.rar"));
        _rar5NoneBytes = File.ReadAllBytes(GetArchivePath("Rar5.none.rar"));
        _multiVolumeBytes =
        [
            File.ReadAllBytes(GetArchivePath("Rar5.multi.none.part01.rar")),
            File.ReadAllBytes(GetArchivePath("Rar5.multi.none.part02.rar")),
            File.ReadAllBytes(GetArchivePath("Rar5.multi.none.part03.rar")),
        ];
    }

    [Benchmark(Description = "Rar stored (m0): full entry read")]
    public void RarStoredFullRead() => ExtractAll(new MemoryStream(_rarNoneBytes));

    [Benchmark(Description = "Rar5 stored (m0): full entry read")]
    public void Rar5StoredFullRead() => ExtractAll(new MemoryStream(_rar5NoneBytes));

    [Benchmark(Description = "Rar stored multi-volume: full entry read")]
    public void RarStoredMultiVolumeFullRead()
    {
        var streams = _multiVolumeBytes
            .Select(b => (Stream)new MemoryStream(b, writable: false))
            .ToArray();
        try
        {
            using var archive = RarArchive.OpenArchive(streams);
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                using var entryStream = entry.OpenEntryStream();
                entryStream.CopyTo(Stream.Null);
            }
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
    }

    [Benchmark(Description = "Rar stored (m0): non-seekable full entry read")]
    public void RarStoredFullReadNonSeekable()
    {
        using var stream = new NonSeekableStream(new MemoryStream(_rarNoneBytes));
        using var reader = ReaderFactory.OpenReader(stream);
        while (reader.MoveToNextEntry())
        {
            if (!reader.Entry.IsDirectory)
            {
                reader.WriteEntryTo(Stream.Null);
            }
        }
    }

    private static void ExtractAll(Stream stream)
    {
        using (stream)
        using (var archive = RarArchive.OpenArchive(stream))
        {
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                using var entryStream = entry.OpenEntryStream();
                entryStream.CopyTo(Stream.Null);
            }
        }
    }
}
