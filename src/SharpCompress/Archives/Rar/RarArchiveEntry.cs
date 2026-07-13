using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Compressors.Rar;
using SharpCompress.Readers;

namespace SharpCompress.Archives.Rar;

public partial class RarArchiveEntry : RarEntry, IArchiveEntry
{
    private readonly ICollection<RarFilePart> parts;
    private readonly RarArchive archive;
    private readonly ReaderOptions readerOptions;

    internal RarArchiveEntry(
        RarArchive archive,
        IEnumerable<RarFilePart> parts,
        ReaderOptions readerOptions
    )
        : base(readerOptions)
    {
        this.parts = parts.ToList();
        this.archive = archive;
        this.readerOptions = readerOptions;
        IsSolid = FileHeader.IsSolid;
    }

    public override CompressionType CompressionType => CompressionType.Rar;

    public IArchive Archive => archive;

    internal override IEnumerable<FilePart> Parts => parts.Cast<FilePart>();

    internal override FileHeader FileHeader => parts.First().FileHeader;

    public override long Crc
    {
        get
        {
            CheckIncomplete();
            return BitConverter.ToUInt32(
                parts.Select(fp => fp.FileHeader).Single(fh => !fh.IsSplitAfter).FileCrc.NotNull(),
                0
            );
        }
    }

    public override long Size
    {
        get
        {
            CheckIncomplete();
            return parts.First().FileHeader.UncompressedSize;
        }
    }

    public override long CompressedSize
    {
        get
        {
            CheckIncomplete();
            return parts.Aggregate(0L, (total, fp) => total + fp.FileHeader.CompressedSize);
        }
    }

    public Stream OpenEntryStream()
    {
        var readStream = new MultiVolumeReadOnlyStream(Parts.Cast<RarFilePart>());
        var isSolidArchive = archive.IsSolid;
        var unpack = archive.AcquireUnpackForEntry(IsRarV3, isSolidArchive, out var ownsUnpack);
        Action? onDispose = isSolidArchive ? archive.ReleaseSolidEntryStream : null;
        RarStream? stream = null;
        try
        {
            if (IsRarV3)
            {
                stream = RarCrcStream.Create(unpack, FileHeader, readStream, ownsUnpack, onDispose);
            }
            else if (FileHeader.FileCrc?.Length > 5)
            {
                stream = RarBLAKE2spStream.Create(
                    unpack,
                    FileHeader,
                    readStream,
                    ownsUnpack,
                    onDispose
                );
            }
            else
            {
                stream = RarCrcStream.Create(unpack, FileHeader, readStream, ownsUnpack, onDispose);
            }

            stream.Initialize();
            return stream;
        }
        catch
        {
            if (stream is not null)
            {
                stream.Dispose();
            }
            else
            {
                readStream.Dispose();
                if (ownsUnpack && unpack is IDisposable disposableUnpack)
                {
                    disposableUnpack.Dispose();
                }

                onDispose?.Invoke();
            }

            throw;
        }
    }

    public bool IsComplete
    {
        get
        {
            var headers = parts.Select(x => x.FileHeader);
            return !headers.First().IsSplitBefore && !headers.Last().IsSplitAfter;
        }
    }

    private void CheckIncomplete()
    {
        if (!readerOptions.DisableCheckIncomplete && !IsComplete)
        {
            throw new IncompleteArchiveException(
                "ArchiveEntry is incomplete and cannot perform this operation."
            );
        }
    }
}
