using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Test.Mocks;
using Xunit;

namespace SharpCompress.Test.Rar;

public class RarAdversarialStreamTests : ArchiveTests
{
    private static readonly string[] MultiVolumeParts =
    [
        "Rar.multi.part01.rar",
        "Rar.multi.part02.rar",
        "Rar.multi.part03.rar",
        "Rar.multi.part04.rar",
        "Rar.multi.part05.rar",
        "Rar.multi.part06.rar",
    ];

    [Fact(Timeout = 30_000)]
    public void Rar_MultiVolume_TruncatedLastPart_ThrowsIncompleteArchiveException()
    {
        var streams = OpenTruncatedMultiVolume(percentOfLastPart: 50);
        try
        {
            using var archive = RarArchive.OpenArchive(streams);
            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    using var entryStream = entry.OpenEntryStream();
                    entryStream.CopyTo(Stream.Null);
                }
            });

            AssertIncompleteOrWrapped(exception);
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
    }

    [Fact(Timeout = 30_000)]
    public async Task Rar_MultiVolume_TruncatedLastPart_ThrowsIncompleteArchiveException_Async()
    {
        var streams = OpenTruncatedMultiVolume(percentOfLastPart: 50);
        try
        {
            await using var archive = await RarArchive.OpenAsyncArchive(streams);
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await foreach (var entry in archive.EntriesAsync)
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }

                    await using var entryStream = await entry.OpenEntryStreamAsync();
                    await entryStream.CopyToAsync(Stream.Null);
                }
            });

            AssertIncompleteOrWrapped(exception);
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [Theory]
    [InlineData("Rar.encrypted_filesOnly.rar", "test", 1)]
    [InlineData("Rar.encrypted_filesOnly.rar", "test", 7)]
    [InlineData("Rar5.encrypted_filesOnly.rar", "test", 1)]
    [InlineData("Rar5.encrypted_filesOnly.rar", "test", 16)]
    public void Rar_Encrypted_ChunkyReads_MatchBaseline(
        string archiveName,
        string password,
        int chunkSize
    )
    {
        var bytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, archiveName));
        var options = ReaderOptions.ForExternalStream with { Password = password };

        var baseline = ExtractEntryBytes(new MemoryStream(bytes, writable: false), options);
        var chunked = ExtractEntryBytes(
            new ChunkyReadStream(new MemoryStream(bytes, writable: false), chunkSize),
            options
        );

        Assert.Equal(baseline.Count, chunked.Count);
        foreach (var key in baseline.Keys)
        {
            Assert.Equal(baseline[key], chunked[key]);
        }
    }

    [Theory]
    [InlineData("Rar.encrypted_filesOnly.rar", "test", 1)]
    [InlineData("Rar5.encrypted_filesOnly.rar", "test", 7)]
    public async Task Rar_Encrypted_ChunkyReads_MatchBaseline_Async(
        string archiveName,
        string password,
        int chunkSize
    )
    {
        var bytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, archiveName));
        var options = ReaderOptions.ForExternalStream with { Password = password };

        var baseline = await ExtractEntryBytesAsync(
            new MemoryStream(bytes, writable: false),
            options
        );
        var chunked = await ExtractEntryBytesAsync(
            new ChunkyReadStream(new MemoryStream(bytes, writable: false), chunkSize),
            options
        );

        Assert.Equal(baseline.Count, chunked.Count);
        foreach (var key in baseline.Keys)
        {
            Assert.Equal(baseline[key], chunked[key]);
        }
    }

    [Fact(Timeout = 30_000)]
    public async Task Rar_AsyncExtraction_RespectsCancellationDuringRead()
    {
        var archiveBytes = await File.ReadAllBytesAsync(
            Path.Combine(TEST_ARCHIVES_PATH, "Rar.rar")
        );
        await using var archive = await RarArchive.OpenAsyncArchive(new MemoryStream(archiveBytes));
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var entry in archive.EntriesAsync.WithCancellation(cts.Token))
            {
                if (entry.IsDirectory)
                {
                    continue;
                }

                await using var entryStream = await entry.OpenEntryStreamAsync(cts.Token);
                var buffer = new byte[256];
                _ = await entryStream.ReadAsync(buffer, cts.Token);
                cts.Cancel();
                _ = await entryStream.ReadAsync(buffer, cts.Token);
            }
        });
    }

    [Fact(Timeout = 30_000)]
    public void SevenZip_Truncated_Throws()
    {
        var path = Path.Combine(TEST_ARCHIVES_PATH, "7Zip.solid.7z");
        using var fileStream = File.OpenRead(path);
        using var truncated = TruncatedStream.AtPercent(fileStream, 50, leaveOpen: false);

        Assert.ThrowsAny<Exception>(() =>
        {
            using var archive = SevenZipArchive.OpenArchive(truncated);
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                using var entryStream = entry.OpenEntryStream();
                entryStream.CopyTo(Stream.Null);
            }
        });
    }

    [Fact(Timeout = 30_000)]
    public void Zip_Truncated_Throws()
    {
        var path = Path.Combine(TEST_ARCHIVES_PATH, "Zip.deflate.zip");
        using var fileStream = File.OpenRead(path);
        using var truncated = TruncatedStream.AtPercent(fileStream, 50, leaveOpen: false);

        Assert.ThrowsAny<Exception>(() =>
        {
            using var archive = ArchiveFactory.OpenArchive(truncated);
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                using var entryStream = entry.OpenEntryStream();
                entryStream.CopyTo(Stream.Null);
            }
        });
    }

    private Stream[] OpenTruncatedMultiVolume(int percentOfLastPart)
    {
        var streams = new List<Stream>();
        for (var i = 0; i < MultiVolumeParts.Length; i++)
        {
            var path = Path.Combine(TEST_ARCHIVES_PATH, MultiVolumeParts[i]);
            var bytes = File.ReadAllBytes(path);
            if (i == MultiVolumeParts.Length - 1)
            {
                var keep = Math.Max(1, bytes.Length * percentOfLastPart / 100);
                Array.Resize(ref bytes, keep);
            }

            streams.Add(new MemoryStream(bytes, writable: false));
        }

        return streams.ToArray();
    }

    private static void AssertIncompleteOrWrapped(Exception exception)
    {
        var current = exception;
        while (current is not null)
        {
            if (current is IncompleteArchiveException)
            {
                Assert.Contains("Unexpected end of stream", current.Message);
                return;
            }

            current = current.InnerException;
        }

        Assert.Fail(
            $"Expected IncompleteArchiveException, got {exception.GetType().Name}: {exception.Message}"
        );
    }

    private static Dictionary<string, byte[]> ExtractEntryBytes(
        Stream stream,
        ReaderOptions options
    )
    {
        var results = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        using var archive = RarArchive.OpenArchive(stream, options);
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            using var entryStream = entry.OpenEntryStream();
            using var ms = new MemoryStream();
            entryStream.CopyTo(ms);
            results[entry.Key!] = ms.ToArray();
        }

        return results;
    }

    private static async Task<Dictionary<string, byte[]>> ExtractEntryBytesAsync(
        Stream stream,
        ReaderOptions options
    )
    {
        var results = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        await using var archive = await RarArchive.OpenAsyncArchive(stream, options);
        await foreach (var entry in archive.EntriesAsync)
        {
            if (entry.IsDirectory)
            {
                continue;
            }

            await using var entryStream = await entry.OpenEntryStreamAsync();
            using var ms = new MemoryStream();
            await entryStream.CopyToAsync(ms);
            results[entry.Key!] = ms.ToArray();
        }

        return results;
    }
}
