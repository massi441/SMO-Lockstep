using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lockstep.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal record struct PacketHeader
{
    public PacketType Type;
    public byte Flags;
    public byte Version;
    public ushort RoomId;
    public ushort PayloadSize;

    public const uint Magic = 0x534D4F4C; // "SMOL"

    public PacketHeader WithSizeType(ushort payloadSize, PacketType type)
    {
        return this with { PayloadSize = payloadSize, Type = type };
    }

    public PacketHeader WithSize(ushort payloadSize)
    {
        return this with { PayloadSize = payloadSize };
    }

    public PacketHeader WithType(PacketType type)
    {
        return this with { Type = type };
    }

    /// <summary>
    /// Returns the size of a packet header from incoming clients.
    /// Note: The size returned by this function includes the magic number as part of the total size
    /// </summary>
    /// <returns>The number of bytes of the size of a packet header sent by clients</returns>
    public static int SizeOf()
    {
        return SizeOfMagic() + Unsafe.SizeOf<PacketHeader>();
    }

    public static int SizeOfMagic()
    {
        return sizeof(uint);
    }
}
