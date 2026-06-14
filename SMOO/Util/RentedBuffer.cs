using System.Buffers;

namespace SMOO.Util;

/// <summary>
/// Represents a byte buffer that was rented from the array pool.
/// </summary>
internal struct RentedBuffer
{
    /// <summary>
    /// The reference to the rented array
    /// </summary>
    public byte[] RentRef { get; private set; }

    /// <summary>
    /// The actual amount of bytes used in the buffer, as a rented Array can be bigger than the capacity requested
    /// </summary>
    public int UsedBytes { get; private set; }

    /// <summary>
    /// A span pointing at the start of the rented buffer, with the size of the used bytes in the rented buffer
    /// </summary>
    public readonly Span<byte> UsedSpan => RentRef.AsSpan(0, UsedBytes);

    public RentedBuffer(int size)
    {
        RentRef = ArrayPool<byte>.Shared.Rent(size);
        UsedBytes = size;
    }

    public void Restrict(int size)
    {
        UsedBytes = Math.Min(size, UsedBytes);
    }

    public readonly Span<byte> SpanAt(int offset)
    {
        return RentRef.AsSpan(offset, UsedBytes - offset);
    }

    public static implicit operator Span<byte>(RentedBuffer buffer)
    {
        return buffer.UsedSpan;
    }

    public readonly void Return()
    {
        if (RentRef == null)
        {
            return;
        } 

        ArrayPool<byte>.Shared.Return(RentRef);
    }

    public static RentedBuffer Move(ref RentedBuffer other)
    {
        RentedBuffer newBuffer = new RentedBuffer()
        {
            RentRef = other.RentRef,
            UsedBytes = other.UsedBytes,
        };

        other.RentRef = null!;
        other.UsedBytes = 0;

        return newBuffer;
    }

    public void Dispose()
    {
        if (RentRef != null)
        {
            ArrayPool<byte>.Shared.Return(RentRef);
            RentRef = null!;
            UsedBytes = 0;
        }
    }
}
