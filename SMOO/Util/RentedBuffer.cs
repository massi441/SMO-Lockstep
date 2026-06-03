namespace SMOO.Util;

/// <summary>
/// Represents a buffer that was rented from the array pool.
/// </summary>
/// <typeparam name="T">Type of element in the array</typeparam>
internal readonly struct RentedBuffer<T>
{
    /// <summary>
    /// The reference to the rented array
    /// </summary>
    public readonly T[] Ref;

    /// <summary>
    /// The actual amount of bytes used in the buffer, as a rented Array can be bigger than the capacity requested
    /// </summary>
    public readonly int UsedBytes;

    public readonly Span<T> Span => Ref.AsSpan(0, UsedBytes);
    public readonly Memory<T> Memory => Ref.AsMemory(0, UsedBytes);

    public RentedBuffer(T[] reference, int usedBytes)
    {
        Ref = reference;
        UsedBytes = usedBytes;
    }
}
