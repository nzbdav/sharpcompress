namespace SharpCompress.Common;

internal enum ChecksumKind
{
    Crc32,
}

internal readonly record struct ChecksumDescriptor(
    ChecksumKind Kind,
    long ExpectedValue,
    bool IsAvailable
);
