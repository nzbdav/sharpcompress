using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCompress.Common.SevenZip;

internal class SevenZipFilePart : FilePart
{
    private CompressionType? _type;
    private bool? _isEncrypted;
    private readonly Stream _stream;
    private readonly ArchiveDatabase _database;

    internal SevenZipFilePart(
        Stream stream,
        ArchiveDatabase database,
        int index,
        CFileItem fileEntry,
        IArchiveEncoding archiveEncoding
    )
        : base(archiveEncoding)
    {
        _stream = stream;
        _database = database;
        Index = index;
        Header = fileEntry;
        if (Header.HasStream)
        {
            FolderIndex = database._fileIndexToFolderIndexMap[index];
            Folder = database._folders[FolderIndex];
        }
        else
        {
            FolderIndex = -1;
        }
    }

    internal CFileItem Header { get; }
    internal CFolder? Folder { get; }
    internal int FolderIndex { get; }

    internal override string FilePartName => Header.Name;

    internal override Stream? GetRawStream() => null;

    internal override Stream GetCompressedStream()
    {
        if (!Header.HasStream)
        {
            return Stream.Null;
        }

        var skipSize = _database._fileInFolderOffset[Index];
        return _database.GetFolderStreamCached(
            _stream,
            FolderIndex,
            skipSize,
            Header.Size,
            _database.PasswordProvider
        );
    }

    internal override async ValueTask<Stream?> GetCompressedStreamAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (!Header.HasStream)
        {
            return Stream.Null;
        }

        var skipSize = _database._fileInFolderOffset[Index];
        return await _database
            .GetFolderStreamCachedAsync(
                _stream,
                FolderIndex,
                skipSize,
                Header.Size,
                _database.PasswordProvider,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public CompressionType CompressionType
    {
        get
        {
            _type ??= GetCompression();
            return _type.Value;
        }
    }

    private const uint K_COPY = 0x0;
    private const uint K_LZMA2 = 0x21;
    private const uint K_LZMA = 0x030101;
    private const uint K_PPMD = 0x030401;
    private const uint K_B_ZIP2 = 0x040202;

    private CompressionType GetCompression()
    {
        if (Header.IsDir)
        {
            return CompressionType.None;
        }

        var coder = Folder.NotNull()._coders[0];
        return coder._methodId._id switch
        {
            K_COPY => CompressionType.None,
            K_LZMA or K_LZMA2 => CompressionType.LZMA,
            K_PPMD => CompressionType.PPMd,
            K_B_ZIP2 => CompressionType.BZip2,
            _ => throw new InvalidFormatException(),
        };
    }

    internal bool IsEncrypted
    {
        get
        {
            _isEncrypted ??=
                !Header.IsDir
                && Folder?._coders.FindIndex(c => c._methodId._id == CMethodId.K_AES_ID) != -1;
            return _isEncrypted.Value;
        }
    }
}
