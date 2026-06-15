using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Crypto;

namespace SharpCompress.Common;

internal sealed class ChecksumValidationStream : Stream
{
    private readonly Stream _stream;
    private readonly ChecksumDescriptor _checksum;
    private readonly string _entryName;
    private readonly uint[] _crc32Table;
    private uint _seed = Crc32Stream.DEFAULT_SEED;
    private bool _validated;

    internal ChecksumValidationStream(Stream stream, ChecksumDescriptor checksum, string? entryName)
    {
        _stream = stream;
        _checksum = checksum;
        _entryName = string.IsNullOrEmpty(entryName) ? "Entry" : entryName!;
        _crc32Table = Crc32Stream.InitializeTable(Crc32Stream.DEFAULT_POLYNOMIAL);
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => throw new NotSupportedException();
    }

    public override void Flush() => _stream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _stream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _stream.Read(buffer, offset, count);
        UpdateAndValidateAtEof(buffer.AsSpan(offset, read), read);
        return read;
    }

#if !LEGACY_DOTNET
    public override int Read(Span<byte> buffer)
    {
        var read = _stream.Read(buffer);
        UpdateAndValidateAtEof(buffer[..read], read);
        return read;
    }
#endif

    public override int ReadByte()
    {
        var value = _stream.ReadByte();
        if (value == -1)
        {
            Validate();
        }
        else
        {
            _seed = Crc32Stream.CalculateCrc(_crc32Table, _seed, (byte)value);
        }

        return value;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        var read = await _stream
            .ReadAsync(buffer, offset, count, cancellationToken)
            .ConfigureAwait(false);
        UpdateAndValidateAtEof(buffer.AsSpan(offset, read), read);
        return read;
    }

#if !LEGACY_DOTNET
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        var read = await _stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        UpdateAndValidateAtEof(buffer.Span[..read], read);
        return read;
    }
#endif

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    private void UpdateAndValidateAtEof(ReadOnlySpan<byte> buffer, int read)
    {
        if (read > 0)
        {
            _seed = Crc32Stream.CalculateCrc(_crc32Table, _seed, buffer);
            return;
        }

        Validate();
    }

    private void Validate()
    {
        if (_validated)
        {
            return;
        }

        _validated = true;

        if (_checksum.Kind != ChecksumKind.Crc32)
        {
            return;
        }

        var actual = ~_seed;
        var expected = unchecked((uint)_checksum.ExpectedValue);
        if (actual != expected)
        {
            throw new InvalidFormatException(
                $"CRC mismatch for entry '{_entryName}'. Expected 0x{expected:X8}, actual 0x{actual:X8}."
            );
        }
    }
}
