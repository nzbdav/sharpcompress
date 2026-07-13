using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Crypto;

namespace SharpCompress.Common.Rar;

internal sealed class AsyncRarCryptoBinaryReader : AsyncRarCrcBinaryReader
{
    private BlockTransformer _rijndael = default!;
    private RarAesDecryptCarry _carry;
    private long _readCount;

    private AsyncRarCryptoBinaryReader(Stream stream)
        : base(stream)
    {
        _carry = new RarAesDecryptCarry();
    }

    public static async ValueTask<AsyncRarCryptoBinaryReader> Create(
        Stream stream,
        ICryptKey cryptKey,
        byte[]? salt = null
    )
    {
        var binary = new AsyncRarCryptoBinaryReader(stream);
        if (salt == null)
        {
            salt = await binary
                .ReadBytesAsyncBase(EncryptionConstV5.SIZE_SALT30)
                .ConfigureAwait(false);
            binary._readCount += EncryptionConstV5.SIZE_SALT30;
        }
        binary._rijndael = new BlockTransformer(cryptKey.Transformer(salt));
        return binary;
    }

    public override long CurrentReadByteCount
    {
        get => _readCount;
        protected set
        {
            // ignore
        }
    }

    public override void Mark() => _readCount = 0;

    public override async ValueTask<byte> ReadByteAsync(
        CancellationToken cancellationToken = default
    )
    {
        var bytes = await ReadAndDecryptBytesAsync(1, cancellationToken).ConfigureAwait(false);
        return bytes[0];
    }

    private ValueTask<byte[]> ReadBytesAsyncBase(int count) => base.ReadBytesAsync(count);

    public override async ValueTask<byte[]> ReadBytesAsync(
        int count,
        CancellationToken cancellationToken = default
    )
    {
        return await ReadAndDecryptBytesAsync(count, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<byte[]> ReadAndDecryptBytesAsync(
        int count,
        CancellationToken cancellationToken
    )
    {
        var decryptedBytes = new byte[count];
        if (count == 0)
        {
            return decryptedBytes;
        }

        var written = _carry.Read(decryptedBytes);
        if (written < count)
        {
            var sizeToRead = count - written;
            var alignedSize = sizeToRead + ((~sizeToRead + 1) & 0xf);
            var cipherText = await ReadBytesNoCrcAsync(alignedSize, cancellationToken)
                .ConfigureAwait(false);
            var plainText = ArrayPool<byte>.Shared.Rent(alignedSize);
            try
            {
                _rijndael.Process(cipherText, 0, alignedSize, plainText, 0);
                plainText.AsSpan(0, sizeToRead).CopyTo(decryptedBytes.AsSpan(written));
                if (alignedSize > sizeToRead)
                {
                    _carry.Store(plainText.AsSpan(sizeToRead, alignedSize - sizeToRead));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(plainText);
            }
        }

        UpdateCrc(decryptedBytes, 0, count);
        _readCount += count;
        return decryptedBytes;
    }

    public void ClearQueue() => _carry.Clear();

    public void SkipQueue()
    {
        BaseStream.Position += _carry.Count;
        ClearQueue();
    }
}
