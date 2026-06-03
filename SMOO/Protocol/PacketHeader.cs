using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SMOO.Util;

namespace SMOO.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal record struct PacketHeader
{
    public readonly uint Magic = Config.Magic;
    public required PacketType Type;
    public required byte Flags;
    public required byte Version;
    public required ushort RoomId;
    public required ushort PayloadSize;

    public PacketHeader()
    {

    }

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

    public static int SizeOf()
    {
        return Unsafe.SizeOf<PacketHeader>();
    }
}
