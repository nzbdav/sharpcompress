using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Crypto;

namespace SharpCompress.Common.Rar;

internal sealed class RarCryptoWrapper : Stream
{
    private readonly Stream _actualStream;
    private readonly BlockTransformer _rijndael;
    private RarAesDecryptCarry _carry;

    public RarCryptoWrapper(Stream actualStream, byte[] salt, ICryptKey key)
    {
        _actualStream = actualStream;
        _rijndael = new BlockTransformer(key.Transformer(salt));
        _carry = new RarAesDecryptCarry();
    }

    public override void Flush() { }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAndDecrypt(buffer, offset, count);

    public int ReadAndDecrypt(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        var written = _carry.Read(buffer.AsSpan(offset, count));
        if (written == count)
        {
            return count;
        }

        var sizeToRead = count - written;
        var alignedSize = sizeToRead + ((~sizeToRead + 1) & 0xf);
        var cipherText = ArrayPool<byte>.Shared.Rent(alignedSize);
        var plainText = ArrayPool<byte>.Shared.Rent(alignedSize);
        try
        {
            try
            {
                _actualStream.ReadExactly(cipherText, 0, alignedSize);
            }
            catch (EndOfStreamException e)
            {
                throw new InvalidFormatException("Unexpected end of encrypted stream", e);
            }

            _rijndael.Process(cipherText, 0, alignedSize, plainText, 0);
            plainText.AsSpan(0, sizeToRead).CopyTo(buffer.AsSpan(offset + written, sizeToRead));
            if (alignedSize > sizeToRead)
            {
                _carry.Store(plainText.AsSpan(sizeToRead, alignedSize - sizeToRead));
            }

            return count;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(cipherText);
            ArrayPool<byte>.Shared.Return(plainText);
        }
    }

    public override Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    ) => ReadAndDecryptAsync(buffer, offset, count, cancellationToken).AsTask();

    private async ValueTask<int> ReadAndDecryptAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (count == 0)
        {
            return 0;
        }

        var written = _carry.Read(buffer.AsSpan(offset, count));
        if (written == count)
        {
            return count;
        }

        var sizeToRead = count - written;
        var alignedSize = sizeToRead + ((~sizeToRead + 1) & 0xf);
        var cipherText = ArrayPool<byte>.Shared.Rent(alignedSize);
        var plainText = ArrayPool<byte>.Shared.Rent(alignedSize);
        try
        {
            try
            {
                await _actualStream
                    .ReadExactlyAsync(cipherText.AsMemory(0, alignedSize), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (EndOfStreamException e)
            {
                throw new InvalidFormatException("Unexpected end of encrypted stream", e);
            }

            _rijndael.Process(cipherText, 0, alignedSize, plainText, 0);
            plainText.AsSpan(0, sizeToRead).CopyTo(buffer.AsSpan(offset + written, sizeToRead));
            if (alignedSize > sizeToRead)
            {
                _carry.Store(plainText.AsSpan(sizeToRead, alignedSize - sizeToRead));
            }

            return count;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(cipherText);
            ArrayPool<byte>.Shared.Return(plainText);
        }
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try
        {
            var bytesRead = await ReadAndDecryptAsync(array, 0, buffer.Length, cancellationToken)
                .ConfigureAwait(false);
            new ReadOnlySpan<byte>(array, 0, bytesRead).CopyTo(buffer.Span);
            return bytesRead;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _rijndael.Dispose();
        }

        base.Dispose(disposing);
    }
}
