using System.Buffers;
using System.Runtime.InteropServices;

namespace SMOO.Util;

/// <summary>
/// Represents a byte buffer that was rented from the array pool.
/// </summary>
internal readonly struct RentedBuffer
{
    /// <summary>
    /// The reference to the rented array
    /// </summary>
    public byte[] Ref { get; init; }

    /// <summary>
    /// The actual amount of bytes used in the buffer, as a rented Array can be bigger than the capacity requested
    /// </summary>
    public int UsedBytes { get; init; }

    /// <summary>
    /// A span pointing at the start of the rented buffer, with the size of the used bytes in the rented buffer
    /// </summary>
    public readonly Span<byte> Span => Ref.AsSpan(0, UsedBytes);

    /// <summary>
    /// A memory view pointing at the start of the rented buffer, with the size of the used bytes in the rented buffer
    /// </summary>
    public readonly Memory<byte> Memory => Ref.AsMemory(0, UsedBytes);

    public RentedBuffer(byte[] rentRef, int size)
    {
        Ref = rentRef;
        UsedBytes = size;
    }

    public RentedBuffer(int size) : this(ArrayPool<byte>.Shared.Rent(size), size)
    {

    }

    public Span<byte> SpanAt(int offset)
    {
        return Ref.AsSpan(offset, UsedBytes - offset);
    }

    public void Return()
    {
        ArrayPool<byte>.Shared.Return(Ref);
    }

    public void Write<T>(in T structure) where T : struct
    {
        MemoryMarshal.Write(Ref, in structure);
    }
}
