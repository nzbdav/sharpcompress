using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Test.Mocks;
using Xunit;

namespace SharpCompress.Test.Readers;

public class EntryStreamDisposeTests : TestBase
{
    [Fact]
    public void Dispose_Default_DrainsRemainingEntryBytes()
    {
        var archiveBytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar"));
        var counting = new CountingReadStream(
            new ForwardOnlyStream(new MemoryStream(archiveBytes)),
            leaveOpen: false
        );
        using var reader = ReaderFactory.OpenReader(counting, ReaderOptions.ForExternalStream);

        reader.MoveToNextEntry().Should().BeTrue();
        while (reader.Entry.IsDirectory)
        {
            reader.MoveToNextEntry().Should().BeTrue();
        }

        var afterOpen = counting.BytesRead;
        using (var entryStream = reader.OpenEntryStream())
        {
            var buffer = new byte[16];
            var read = entryStream.Read(buffer, 0, buffer.Length);
            read.Should().BeGreaterThan(0);
        }

        counting.BytesRead.Should().BeGreaterThan(afterOpen + 16);
        reader.Cancelled.Should().BeFalse();
        reader.MoveToNextEntry().Should().BeTrue();
    }

    [Fact]
    public void Dispose_Default_SeekableSourceSeeksPastRemainingEntryBytes()
    {
        var archiveBytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar"));
        var counting = new CountingReadStream(new MemoryStream(archiveBytes), leaveOpen: false);
        using var reader = ReaderFactory.OpenReader(counting, ReaderOptions.ForExternalStream);

        reader.MoveToNextEntry().Should().BeTrue();
        while (reader.Entry.IsDirectory)
        {
            reader.MoveToNextEntry().Should().BeTrue();
        }

        long bytesAfterPartialRead;
        using (var entryStream = reader.OpenEntryStream())
        {
            var buffer = new byte[16];
            entryStream.Read(buffer, 0, buffer.Length).Should().BeGreaterThan(0);
            bytesAfterPartialRead = counting.BytesRead;
        }

        counting.BytesRead.Should().Be(bytesAfterPartialRead);
        reader.Cancelled.Should().BeFalse();
        reader.MoveToNextEntry().Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_Default_SeekableSourceSeeksPastRemainingEntryBytes()
    {
        var archiveBytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar"));
        var counting = new CountingReadStream(new MemoryStream(archiveBytes), leaveOpen: false);
        await using var reader = await ReaderFactory.OpenAsyncReader(
            counting,
            ReaderOptions.ForExternalStream
        );

        (await reader.MoveToNextEntryAsync()).Should().BeTrue();
        while (reader.Entry.IsDirectory)
        {
            (await reader.MoveToNextEntryAsync()).Should().BeTrue();
        }

        long bytesAfterPartialRead;
        await using (var entryStream = await reader.OpenEntryStreamAsync())
        {
            var buffer = new byte[16];
            (await entryStream.ReadAsync(buffer)).Should().BeGreaterThan(0);
            bytesAfterPartialRead = counting.BytesRead;
        }

        counting.BytesRead.Should().Be(bytesAfterPartialRead);
        reader.Cancelled.Should().BeFalse();
        (await reader.MoveToNextEntryAsync()).Should().BeTrue();
    }

    [Fact]
    public void Dispose_CancelOnEntryStreamDispose_DoesNotDrain()
    {
        var archiveBytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar"));
        var counting = new CountingReadStream(new MemoryStream(archiveBytes), leaveOpen: false);
        using var reader = ReaderFactory.OpenReader(
            counting,
            ReaderOptions.ForExternalStream.WithCancelOnEntryStreamDispose(true)
        );

        reader.MoveToNextEntry().Should().BeTrue();
        while (reader.Entry.IsDirectory)
        {
            reader.MoveToNextEntry().Should().BeTrue();
        }

        long bytesAfterPartialRead;
        using (var entryStream = reader.OpenEntryStream())
        {
            var buffer = new byte[16];
            var read = entryStream.Read(buffer, 0, buffer.Length);
            read.Should().BeGreaterThan(0);
            bytesAfterPartialRead = counting.BytesRead;
        }

        counting.BytesRead.Should().Be(bytesAfterPartialRead);
        reader.Cancelled.Should().BeTrue();
        Assert.Throws<ReaderCancelledException>(() => reader.MoveToNextEntry());
    }

    [Fact]
    public async Task DisposeAsync_CancelOnEntryStreamDispose_DoesNotDrain()
    {
        var archiveBytes = File.ReadAllBytes(Path.Combine(TEST_ARCHIVES_PATH, "Tar.tar"));
        var counting = new CountingReadStream(new MemoryStream(archiveBytes), leaveOpen: false);
        await using var reader = await ReaderFactory.OpenAsyncReader(
            counting,
            ReaderOptions.ForExternalStream.WithCancelOnEntryStreamDispose(true)
        );

        (await reader.MoveToNextEntryAsync()).Should().BeTrue();
        while (reader.Entry.IsDirectory)
        {
            (await reader.MoveToNextEntryAsync()).Should().BeTrue();
        }

        long bytesAfterPartialRead;
        await using (var entryStream = await reader.OpenEntryStreamAsync())
        {
            var buffer = new byte[16];
            var read = await entryStream.ReadAsync(buffer);
            read.Should().BeGreaterThan(0);
            bytesAfterPartialRead = counting.BytesRead;
        }

        counting.BytesRead.Should().Be(bytesAfterPartialRead);
        reader.Cancelled.Should().BeTrue();
        await Assert.ThrowsAsync<ReaderCancelledException>(async () =>
            await reader.MoveToNextEntryAsync()
        );
    }
}
