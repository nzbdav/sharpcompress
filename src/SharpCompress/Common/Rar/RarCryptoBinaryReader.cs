using System;
using System.Buffers;
using System.IO;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Crypto;

namespace SharpCompress.Common.Rar;

internal sealed class RarCryptoBinaryReader : RarCrcBinaryReader
{
    private BlockTransformer _rijndael = default!;
    private RarAesDecryptCarry _carry;
    private long _readCount;

    private RarCryptoBinaryReader(Stream stream)
        : base(stream)
    {
        _carry = new RarAesDecryptCarry();
    }

    public static RarCryptoBinaryReader Create(
        Stream stream,
        ICryptKey cryptKey,
        byte[]? salt = null
    )
    {
        var binary = new RarCryptoBinaryReader(stream);
        if (salt == null)
        {
            salt = binary.ReadBytesBase(EncryptionConstV5.SIZE_SALT30);
            binary._readCount += EncryptionConstV5.SIZE_SALT30;
        }
        binary._rijndael = new BlockTransformer(cryptKey.Transformer(salt));
        return binary;
    }

    // track read count ourselves rather than using the underlying stream since we buffer
    public override long CurrentReadByteCount
    {
        get => _readCount;
        protected set
        {
            // ignore
        }
    }

    public override void Mark() => _readCount = 0;

    public override byte ReadByte() => ReadAndDecryptBytes(1)[0];

    public override byte[] ReadBytes(int count) => ReadAndDecryptBytes(count);

    private byte[] ReadBytesBase(int count) => base.ReadBytes(count);

    private byte[] ReadAndDecryptBytes(int count)
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
            var cipherText = ReadBytesNoCrc(alignedSize);
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
