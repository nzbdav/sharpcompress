using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCompress;

public static class BinaryReaderExtensions
{
    extension(BinaryReader reader)
    {
        public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken = default)
        {
            var buffer = new byte[1];
            await reader
                .BaseStream.ReadExactAsync(buffer, 0, 1, cancellationToken)
                .ConfigureAwait(false);
            return buffer[0];
        }

        public async ValueTask<byte[]> ReadBytesAsync(
            int count,
            CancellationToken cancellationToken = default
        )
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
            }

            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            var bytes = new byte[count];
            await reader
                .BaseStream.ReadExactAsync(bytes, 0, count, cancellationToken)
                .ConfigureAwait(false);
            return bytes;
        }
    }
}
