using System;
using System.IO;
using SharpCompress.Common;

namespace SharpCompress.IO;

/// <summary>
/// Stream wrapper that provides optional ring-buffered reading for non-seekable
/// or forward-only streams, enabling limited backward seeking required by some
/// decompressors and archive formats.
/// </summary>
/// <remarks>
/// <para>
/// Prefer the static <see cref="Create(Stream, int?)"/> / <see cref="CreateNonDisposing"/>
/// factories over constructing this type directly. Factories select passthrough,
/// seekable-delegate, or ring-buffered mode and ownership rules for the underlying stream.
/// See also the internal <c>SeekableSharpCompressStream</c> subclass used for seekable inputs.
/// </para>
///
/// <para><b>Modes</b></para>
/// <list type="bullet">
/// <item>
/// <b>Passthrough</b> (<see cref="CreateNonDisposing"/>): no ring buffer; reads/writes/seeks
/// forward to the underlying stream. <see cref="CanSeek"/> / <see cref="Position"/> /
/// <see cref="Length"/> / write / flush all delegate. Recording APIs are illegal (see below).
/// </item>
/// <item>
/// <b>Ring-buffered</b> (<see cref="Create(Stream, int?)"/> on a non-seekable stream): a
/// <see cref="RingBuffer"/> stores bytes read from the underlying stream so limited backward
/// seeking and format-detection rewind are possible. <see cref="CanSeek"/> is true for buffered
/// position changes within the recorded/buffered window; write and flush are not supported.
/// </item>
/// <item>
/// <b>Seekable delegate</b> (<c>SeekableSharpCompressStream</c> from <see cref="Create(Stream, int?)"/>
/// on a seekable stream): no ring buffer; <see cref="Position"/> / seek / read / write delegate to
/// the underlying stream. <see cref="StartRecording"/> stores the native position;
/// <see cref="Rewind()"/> seeks back to it.
/// </item>
/// </list>
///
/// <para><b>Ownership / <see cref="LeaveStreamOpen"/></b></para>
/// <list type="bullet">
/// <item>
/// <see cref="CreateNonDisposing"/> always sets <see cref="LeaveStreamOpen"/> to
/// <see langword="true"/>: disposing the wrapper never disposes the underlying stream.
/// </item>
/// <item>
/// <see cref="Create(Stream, int?)"/> on a raw (not already wrapped) stream takes ownership
/// (<see cref="LeaveStreamOpen"/> is <see langword="false"/>): disposing the wrapper disposes
/// the underlying stream.
/// </item>
/// <item>
/// When <see cref="Create(Stream, int?)"/> unwraps a passthrough wrapper, the new wrapper
/// preserves non-owning semantics (<see cref="LeaveStreamOpen"/> is <see langword="true"/>)
/// so <see cref="CreateNonDisposing"/> callers do not unexpectedly lose the underlying stream.
/// </item>
/// </list>
///
/// <para><b><see cref="Position"/> semantics</b></para>
/// <list type="bullet">
/// <item>
/// Passthrough and seekable-delegate modes report and set the underlying stream's
/// <see cref="Stream.Position"/>.
/// </item>
/// <item>
/// Ring-buffered modes report <c>_logicalPosition</c>, which can lag the underlying read
/// cursor (<c>streamPosition</c>) after <see cref="Rewind()"/> / <see cref="StopRecording"/>.
/// Subsequent reads replay from the ring buffer until the logical cursor catches up, then
/// continue reading (and recording into the ring) from the underlying stream.
/// </item>
/// </list>
///
/// <para><b>Recording rules (ring-buffered mode)</b></para>
/// <list type="bullet">
/// <item>
/// In ring-buffered mode, bytes read from the underlying stream are always written into the
/// ring (for over-read protection and rewind). <see cref="StartRecording"/> sets the rewind
/// anchor; recording stays active until <see cref="StopRecording"/> or
/// <see cref="Rewind(bool)"/> with <c>stopRecording: true</c>.
/// </item>
/// <item>
/// <see cref="StartRecording"/> anchors at the current underlying read cursor. When
/// <c>minBufferSize</c> is larger than the existing capacity, and a ring was already
/// allocated with a smaller capacity, <see cref="StartRecording"/> throws. When no ring
/// exists yet, capacity is at least <see cref="Common.Constants.RewindableBufferSize"/>
/// (or <c>minBufferSize</c> if larger).
/// </item>
/// <item>
/// <b>Frozen recording:</b> after <see cref="StopRecording"/> or
/// <see cref="Rewind(bool)"/> with <c>stopRecording: true</c>, <c>_isRecording</c> is
/// <see langword="false"/> but the recording anchor (<c>_recordingStartPosition</c>) is kept.
/// Further <see cref="Rewind()"/> calls and seeks within
/// <c>[anchor, streamPosition]</c> remain legal. The anchor is cleared only when a new
/// <see cref="StartRecording"/> begins or the stream is disposed.
/// </item>
/// </list>
///
/// <para><b>Legal call sequences</b></para>
/// <list type="bullet">
/// <item>
/// Typical detection flow: <c>StartRecording → (reads) → Rewind</c> (optionally repeated while
/// still recording) → <c>StopRecording</c> (or <c>Rewind(stopRecording: true)</c>).
/// </item>
/// <item>
/// After freeze: <see cref="Rewind()"/> is still allowed; <see cref="StopRecording"/> throws
/// because recording is no longer active; <see cref="StartRecording"/> may begin a new session.
/// </item>
/// <item>
/// In <b>passthrough</b> mode, <see cref="StartRecording"/>, <see cref="Rewind()"/> /
/// <see cref="Rewind(bool)"/>, and <see cref="StopRecording"/> always throw
/// <see cref="ArchiveOperationException"/>.
/// </item>
/// <item>
/// <see cref="Rewind()"/> also throws if recording was never started, or if the recording
/// anchor has aged out of the ring buffer (overflow).
/// </item>
/// </list>
///
/// <para>
/// Factory unwrap / <c>bufferSize</c> ignore rules are documented on
/// <see cref="Create(Stream, int?)"/> and <see cref="CreateNonDisposing"/>.
/// </para>
/// </remarks>
public partial class SharpCompressStream : Stream, IStreamStack
{
    public virtual Stream BaseStream() => stream;

    private readonly Stream stream;
    private bool isDisposed;
    private long streamPosition;

    // Ring buffer for recording mode and over-read protection.
    // Single unified buffering mechanism for both use cases.
    private RingBuffer? _ringBuffer;
    private long _logicalPosition; // The current logical read position (can be behind streamPosition)

    // Recording state: anchor position when StartRecording was called
    private long? _recordingStartPosition;
    private bool _isRecording;

    // Passthrough mode - no buffering, delegates CanSeek to underlying stream
    private readonly bool _isPassthrough;

    /// <summary>
    /// Gets whether this stream is in passthrough mode (no buffering, delegates to underlying stream).
    /// </summary>
    internal bool IsPassthrough => _isPassthrough;

    /// <summary>
    /// Gets whether to leave the underlying stream open when disposed.
    /// </summary>
    public virtual bool LeaveStreamOpen { get; }

    public SharpCompressStream(Stream stream)
    {
        this.stream = stream;
        _logicalPosition = 0;
    }

    /// <summary>
    /// Private constructor for passthrough mode.
    /// </summary>
    protected SharpCompressStream(
        Stream stream,
        bool leaveStreamOpen,
        bool passthrough,
        int? bufferSize
    )
    {
        this.stream = stream;
        LeaveStreamOpen = leaveStreamOpen;
        _isPassthrough = passthrough;
        _logicalPosition = 0;

        if (bufferSize.HasValue && bufferSize.Value > 0)
        {
            _ringBuffer = new RingBuffer(bufferSize.Value);
        }
    }

    /// <summary>
    /// Gets whether the stream is actively recording reads to the ring buffer.
    /// </summary>
    internal virtual bool IsRecording => _isRecording;

    protected override void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }
        isDisposed = true;
        base.Dispose(disposing);
        if (disposing)
        {
            if (!LeaveStreamOpen)
            {
                stream.Dispose();
            }
            _ringBuffer?.Dispose();
            _ringBuffer = null;
        }
    }

    public void Rewind() => Rewind(false);

    public virtual void Rewind(bool stopRecording)
    {
        if (_isPassthrough)
        {
            throw new ArchiveOperationException(
                "Rewind cannot be called on a passthrough stream. Use Create() first."
            );
        }

        if (_recordingStartPosition is null)
        {
            throw new ArchiveOperationException(
                "Rewind can only be called after StartRecording() has been called."
            );
        }

        // Verify recording anchor is within ring buffer range
        long anchorAge = streamPosition - _recordingStartPosition.Value;
        if (anchorAge > _ringBuffer!.Length)
        {
            throw new ArchiveOperationException(
                $"Cannot rewind: recording anchor is {anchorAge} bytes behind current position, "
                    + $"but ring buffer only holds {_ringBuffer.Length} bytes. "
                    + $"Recording buffer overflow - increase DefaultRollingBufferSize or reduce format detection reads."
            );
        }

        // Rewind logical position to recording anchor
        _logicalPosition = _recordingStartPosition.Value;

        if (stopRecording)
        {
            _isRecording = false;
            // Note: We keep _recordingStartPosition so Rewind() can be called again
            // (frozen recording mode). The anchor is only cleared when a new recording
            // starts or the stream is disposed.
        }
    }

    public virtual void StopRecording()
    {
        if (_isPassthrough)
        {
            throw new ArchiveOperationException(
                "StopRecording cannot be called on a passthrough stream. Use Create() first."
            );
        }
        if (!IsRecording)
        {
            throw new ArchiveOperationException(
                "StopRecording can only be called when recording is active."
            );
        }

        // Mark that we're no longer actively recording
        _isRecording = false;

        // Rewind to recording anchor position
        _logicalPosition = _recordingStartPosition!.Value;

        // Note: We keep _recordingStartPosition so future Rewind() calls still work
        // (frozen recording mode). The anchor is only cleared when a new recording
        // starts or the stream is disposed.
    }

    /// <summary>
    /// Begins recording reads so that <see cref="Rewind()"/> can replay them.
    /// </summary>
    /// <param name="minBufferSize">
    /// Minimum ring buffer capacity in bytes. When provided and larger than
    /// <see cref="Common.Constants.RewindableBufferSize"/>, the ring buffer is allocated
    /// with this size. Pass the largest amount of compressed data that may be consumed
    /// during format detection before the first rewind. Defaults to
    /// <see cref="Common.Constants.RewindableBufferSize"/> when null or not supplied.
    /// </param>
    public virtual void StartRecording(int? minBufferSize = null)
    {
        if (_isPassthrough)
        {
            throw new ArchiveOperationException(
                "StartRecording cannot be called on a passthrough stream. Use Create() first."
            );
        }
        if (IsRecording)
        {
            throw new ArchiveOperationException(
                "StartRecording can only be called when not already recording."
            );
        }

        // Allocate ring buffer with the requested minimum size (at least the global default).
        if (_ringBuffer is null)
        {
            var requiredSize =
                minBufferSize.GetValueOrDefault() > Constants.RewindableBufferSize
                    ? minBufferSize.GetValueOrDefault()
                    : Constants.RewindableBufferSize;
            _ringBuffer = new RingBuffer(requiredSize);
        }
        else if (minBufferSize.HasValue && minBufferSize.Value > _ringBuffer.Capacity)
        {
            throw new ArchiveOperationException(
                $"StartRecording requires a ring buffer of at least {minBufferSize.Value} bytes, but the stream was created with capacity {_ringBuffer.Capacity}."
            );
        }

        // Mark current position as recording anchor
        _recordingStartPosition = streamPosition;
        _logicalPosition = streamPosition;
        _isRecording = true;
    }

    public override bool CanRead => true;

    public override bool CanSeek => !_isPassthrough || stream.CanSeek;

    public override bool CanWrite => _isPassthrough && stream.CanWrite;

    public override void Flush()
    {
        if (_isPassthrough)
        {
            stream.Flush();
            return;
        }
        throw new NotSupportedException();
    }

    public override long Length
    {
        get
        {
            if (_isPassthrough)
            {
                return stream.Length;
            }

            if (_ringBuffer is not null)
            {
                return _ringBuffer.Length;
            }
            throw new NotSupportedException();
        }
    }

    public override long Position
    {
        get
        {
            // In passthrough mode, delegate to underlying stream
            if (_isPassthrough)
            {
                return stream.Position;
            }
            // Use logical position (same for both recording and ring buffer modes)
            return _logicalPosition;
        }
        set
        {
            // In passthrough mode, delegate to underlying stream
            if (_isPassthrough)
            {
                stream.Position = value;
                return;
            }
            SeekToPosition(value);
        }
    }

    private void SeekToPosition(long targetPosition)
    {
        if (!TrySetBufferedPosition(targetPosition))
        {
            if (_recordingStartPosition is not null)
            {
                throw new NotSupportedException(
                    $"Cannot seek to position {targetPosition}. Valid recorded range: "
                        + $"[{_recordingStartPosition.Value}, {streamPosition}]"
                );
            }

            if (_ringBuffer is not null)
            {
                long ringBufferStart = streamPosition - _ringBuffer.Length;
                throw new NotSupportedException(
                    $"Cannot seek to position {targetPosition}. Valid ring buffer range: "
                        + $"[{ringBufferStart}, {streamPosition}]"
                );
            }

            throw new NotSupportedException("Cannot seek on non-buffered stream.");
        }
    }

    /// <summary>
    /// Attempts to set <see cref="Position"/> within the buffered or recorded range without throwing.
    /// </summary>
    internal virtual bool TrySetBufferedPosition(long targetPosition)
    {
        if (_isPassthrough)
        {
            if (!stream.CanSeek)
            {
                return false;
            }

            stream.Position = targetPosition;
            return true;
        }

        if (_recordingStartPosition is not null)
        {
            if (targetPosition >= _recordingStartPosition.Value && targetPosition <= streamPosition)
            {
                _logicalPosition = targetPosition;
                return true;
            }

            return false;
        }

        if (_ringBuffer is not null)
        {
            long ringBufferStart = streamPosition - _ringBuffer.Length;
            if (targetPosition >= ringBufferStart && targetPosition <= streamPosition)
            {
                _logicalPosition = targetPosition;
                return true;
            }

            return false;
        }

        return false;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        // In passthrough mode, delegate directly to underlying stream
        if (_isPassthrough)
        {
            return stream.Read(buffer, offset, count);
        }

        // If ring buffer exists, use unified buffered read logic
        if (_ringBuffer is not null)
        {
            return ReadWithRingBuffer(buffer, offset, count);
        }

        // No buffering - read directly from stream
        int read = stream.Read(buffer, offset, count);
        streamPosition += read;
        _logicalPosition = streamPosition;
        return read;
    }

    /// <summary>
    /// Reads data using the ring buffer. If logical position is behind stream position,
    /// serves data from the ring buffer first. Handles both recording mode and
    /// over-read protection uniformly.
    /// </summary>
    private int ReadWithRingBuffer(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;

        // If logical position is behind stream position, read from ring buffer first
        while (count > 0 && _logicalPosition < streamPosition)
        {
            long bytesFromEnd = streamPosition - _logicalPosition;

            // Verify data is available in ring buffer
            if (!_ringBuffer!.CanReadFromEnd(bytesFromEnd))
            {
                throw new ArchiveOperationException(
                    $"Ring buffer underflow: trying to read {bytesFromEnd} bytes back, "
                        + $"but buffer only holds {_ringBuffer.Length} bytes."
                );
            }

            int available = _ringBuffer.ReadFromEnd(bytesFromEnd, buffer, offset, count);
            totalRead += available;
            offset += available;
            count -= available;
            _logicalPosition += available;
        }

        // If more data needed and we're caught up, read from underlying stream
        if (count > 0 && _logicalPosition == streamPosition)
        {
            // Use async read if stream doesn't support sync reads (e.g., AsyncOnlyStream)
            int read = stream.Read(buffer, offset, count);
            if (read > 0)
            {
                _ringBuffer!.Write(buffer, offset, read);
                streamPosition += read;
                _logicalPosition += read;
                totalRead += read;
            }
        }

        return totalRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        // In passthrough mode, delegate to underlying stream
        if (_isPassthrough)
        {
            return stream.Seek(offset, origin);
        }

        long targetPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => throw new NotSupportedException("Seeking from end is not supported."),
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };

        SeekToPosition(targetPosition);
        return targetPosition;
    }

    public override void SetLength(long value)
    {
        if (_isPassthrough)
        {
            stream.SetLength(value);
            return;
        }
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_isPassthrough)
        {
            stream.Write(buffer, offset, count);
            return;
        }
        throw new NotSupportedException();
    }

    public override int Read(Span<byte> buffer)
    {
        if (_isPassthrough)
        {
            return stream.Read(buffer);
        }
        // Fall back to base implementation for buffered modes
        return base.Read(buffer);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_isPassthrough)
        {
            stream.Write(buffer);
            return;
        }
        throw new NotSupportedException();
    }
}
