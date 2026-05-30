using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lockstep.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PacketHeader
{
    public PacketType Type;
    public byte Version;
    public ushort RoomId;
    public ushort PayloadSize;

    public const uint Magic = 0x534D4F4C; // "SMOL"

    /// <summary>
    /// Returns the size of a packet header from incoming clients.
    /// Note: The size returned by this function includes the magic number as part of the total size
    /// </summary>
    /// <returns>The number of bytes of the size of a packet header sent by clients</returns>
    public static int SizeOf()
    {
        return sizeof(uint) + Unsafe.SizeOf<PacketHeader>();
    }
}
