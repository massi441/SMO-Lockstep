using System.Buffers;
using System.Runtime.CompilerServices;
using SMOO.Protocol;

namespace SMOO.Util;

internal static class MemoryUtil
{
    /// <summary>
    /// Rents a byte buffer from the array pool for a given struct.
    /// </summary>
    /// <typeparam name="T">The type of struct the rented buffer is for</typeparam>
    /// <returns>A rented buffer, large enough to fit the specified struct</returns>
    public static RentedBuffer Rent<T>() where T : struct
    {
        return new RentedBuffer(Unsafe.SizeOf<T>());
    }

    /// <summary>
    /// Returns the payload size of a packet
    /// </summary>
    /// <typeparam name="T">The type of packet</typeparam>
    /// <returns>The size of a packet payload, using the difference in size of the specified struct, and the packet header struct</returns>
    public static ushort PayloadSize<T>() where T : struct
    {
        return (ushort)(Unsafe.SizeOf<T>() - Unsafe.SizeOf<PacketHeader>());
    }
}
