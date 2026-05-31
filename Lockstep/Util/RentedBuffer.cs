namespace Lockstep.Util;

internal readonly struct RentedBuffer<T>
{
    public readonly T[] Buffer;
    public readonly int Count;
}
