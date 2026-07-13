using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.IO;

namespace SharpCompress;

public static class StreamExtensions
{
    extension(Stream stream)
    {
        public void Skip(long advanceAmount)
        {
            // Buffered SharpCompressStream reports CanSeek=true for ring-buffer replay only;
            // arbitrary Position seeks are not safe there.
            if (stream.CanSeek && stream is not SharpCompressStream)
            {
                stream.Position += advanceAmount;
                return;
            }

            using var readOnlySubStream = new ReadOnlySubStream(stream, advanceAmount);
            readOnlySubStream.CopyTo(Stream.Null);
        }

        public void Skip() => stream.CopyTo(Stream.Null);

        public async ValueTask SkipAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await stream.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
        }
    }
}
