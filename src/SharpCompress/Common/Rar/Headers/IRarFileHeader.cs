namespace SharpCompress.Common.Rar.Headers;

/// <summary>
/// Read-only view of a RAR file (or service) header, exposing the metadata needed to
/// locate and describe an entry's packed bytes without extracting the archive.
/// Also backs <see cref="HeaderType.Service"/> / <see cref="HeaderType.NewSub"/> headers;
/// filter on <see cref="IRarHeader.HeaderType"/> to select real files.
/// </summary>
public interface IRarFileHeader : IRarHeader
{
    /// <summary>Entry path within the archive.</summary>
    string FileName { get; }

    /// <summary>True when the entry is a directory.</summary>
    bool IsDirectory { get; }

    /// <summary>RAR compression method; <c>0</c> means stored (m0).</summary>
    byte CompressionMethod { get; }

    /// <summary>True when <see cref="CompressionMethod"/> is <c>0</c> (stored, no compression).</summary>
    bool IsStored { get; }

    /// <summary>Size of the packed bytes for this header/part.</summary>
    long CompressedSize { get; }

    /// <summary>Uncompressed size of the entry.</summary>
    long UncompressedSize { get; }

    /// <summary>Absolute stream offset of the packed bytes (seekable mode only).</summary>
    long DataStartPosition { get; }

    /// <summary>Size of the additional data that follows the header (packed payload).</summary>
    long AdditionalDataSize { get; }

    /// <summary>True when the entry is encrypted.</summary>
    bool IsEncrypted { get; }

    /// <summary>True when the entry uses the solid compression dictionary.</summary>
    bool IsSolid { get; }

    /// <summary>True when the entry's data continues from a previous volume.</summary>
    bool IsSplitBefore { get; }

    /// <summary>True when the entry's data continues into the next volume.</summary>
    bool IsSplitAfter { get; }

    /// <summary>Encryption parameters for this entry, or <c>null</c> when not encrypted.</summary>
    IRarCryptoInfo? CryptoInfo { get; }
}
