using System.IO;
using SharpCompress.Common;

namespace SharpCompress.Compressors.Rar;

internal abstract class MultiVolumeReadOnlyStreamBase : Stream
{
    public byte[]? CurrentCrc { get; protected set; }

    /// <summary>
    /// Validates a volume part read. <see cref="Stream.Read"/> never returns a negative value;
    /// EOF is signaled by 0. A zero read before the part's expected end is a truncated volume.
    /// </summary>
    protected static void ValidateVolumeRead(int read, long currentPosition, long maxPosition)
    {
        if (read == 0 && currentPosition < maxPosition)
        {
            throw new IncompleteArchiveException(
                $"Unexpected end of stream reading RAR volume part. Expected {maxPosition} bytes, got {currentPosition}."
            );
        }
    }
}
