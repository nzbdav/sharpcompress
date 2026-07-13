using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Compressors.LZMA.Utilities;
using SharpCompress.IO;

namespace SharpCompress.Common.SevenZip;

internal sealed partial class ArchiveDatabase
{
    internal async ValueTask<Stream> GetFolderStreamAsync(
        Stream stream,
        CFolder folder,
        IPasswordProvider pw,
        CancellationToken cancellationToken
    )
    {
        var packStreamIndex = folder._firstPackStreamId;
        var folderStartPackPos = GetFolderStreamPos(folder, 0);
        var count = folder._packStreams.Count;
        var packSizes = new long[count];
        for (var j = 0; j < count; j++)
        {
            packSizes[j] = _packSizes[packStreamIndex + j];
        }

        return await DecoderStreamHelper
            .CreateDecoderStreamAsync(
                stream,
                folderStartPackPos,
                packSizes,
                folder,
                pw,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Async twin of <see cref="GetFolderStreamCached"/>. Concurrent opens bypass the cache.
    /// </summary>
    internal async ValueTask<Stream> GetFolderStreamCachedAsync(
        Stream baseStream,
        int folderIndex,
        long targetOffset,
        long entryLength,
        IPasswordProvider pw,
        CancellationToken cancellationToken = default
    )
    {
        if (Interlocked.CompareExchange(ref _folderCacheInUse, 1, 0) != 0)
        {
            return await CreateUncachedEntryStreamAsync(
                    baseStream,
                    folderIndex,
                    targetOffset,
                    entryLength,
                    pw,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        try
        {
            await EnsureFolderStreamAtAsync(
                    baseStream,
                    folderIndex,
                    targetOffset,
                    pw,
                    cancellationToken
                )
                .ConfigureAwait(false);
            var entryStream = new ReadOnlySubStream(
                _cachedFolderStream!,
                entryLength,
                leaveOpen: true
            );
            return new FolderCacheEntryStream(this, entryStream, targetOffset, entryLength);
        }
        catch
        {
            Interlocked.Exchange(ref _folderCacheInUse, 0);
            throw;
        }
    }

    private async ValueTask<ReadOnlySubStream> CreateUncachedEntryStreamAsync(
        Stream baseStream,
        int folderIndex,
        long targetOffset,
        long entryLength,
        IPasswordProvider pw,
        CancellationToken cancellationToken
    )
    {
        var folderStream = await GetFolderStreamAsync(
                baseStream,
                _folders[folderIndex],
                pw,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (targetOffset > 0)
        {
            await folderStream.SkipAsync(targetOffset, cancellationToken).ConfigureAwait(false);
        }
        return new ReadOnlySubStream(folderStream, entryLength, leaveOpen: false);
    }

    private async ValueTask EnsureFolderStreamAtAsync(
        Stream baseStream,
        int folderIndex,
        long targetOffset,
        IPasswordProvider pw,
        CancellationToken cancellationToken
    )
    {
        if (
            _cachedFolderIndex == folderIndex
            && _cachedFolderStream is not null
            && targetOffset >= _cachedPositionInFolder
        )
        {
            var skip = targetOffset - _cachedPositionInFolder;
            if (skip > 0)
            {
                await _cachedFolderStream.SkipAsync(skip, cancellationToken).ConfigureAwait(false);
            }
            _cachedPositionInFolder = targetOffset;
            return;
        }

        if (_cachedFolderStream is not null)
        {
            await _cachedFolderStream.DisposeAsync().ConfigureAwait(false);
            _cachedFolderStream = null;
        }
        _cachedFolderStream = await GetFolderStreamAsync(
                baseStream,
                _folders[folderIndex],
                pw,
                cancellationToken
            )
            .ConfigureAwait(false);
        _cachedFolderIndex = folderIndex;
        if (targetOffset > 0)
        {
            await _cachedFolderStream
                .SkipAsync(targetOffset, cancellationToken)
                .ConfigureAwait(false);
        }
        _cachedPositionInFolder = targetOffset;
    }
}
