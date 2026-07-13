using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;

namespace SharpCompress.Performance.Benchmarks;

/// <summary>
/// Encrypted RAR5 entry read baseline (crypto pipeline findings 2.3/2.4).
/// </summary>
[MemoryDiagnoser]
public class RarEncryptedReadBenchmarks : ArchiveBenchmarkBase
{
    private byte[] _encryptedBytes = null!;
    private ReaderOptions _options = null!;

    [GlobalSetup]
    public void Setup()
    {
        _encryptedBytes = File.ReadAllBytes(GetArchivePath("Rar5.encrypted_filesOnly.rar"));
        _options = ReaderOptions.ForExternalStream with { Password = "test" };
    }

    [Benchmark(Description = "Rar5 encrypted: full entry read")]
    public void Rar5EncryptedFullRead()
    {
        using var stream = new MemoryStream(_encryptedBytes);
        using var archive = RarArchive.OpenArchive(stream, _options);
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            using var entryStream = entry.OpenEntryStream();
            entryStream.CopyTo(Stream.Null);
        }
    }

    [Benchmark(Description = "Rar5 encrypted: non-seekable full entry read")]
    public void Rar5EncryptedFullReadNonSeekable()
    {
        using var stream = new NonSeekableStream(new MemoryStream(_encryptedBytes));
        using var reader = ReaderFactory.OpenReader(stream, _options);
        while (reader.MoveToNextEntry())
        {
            if (!reader.Entry.IsDirectory)
            {
                reader.WriteEntryTo(Stream.Null);
            }
        }
    }
}
