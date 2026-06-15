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
    public ushort SequenceNumber;

    public PacketHeader()
    {

    }

    public PacketHeader WithType(PacketType type)
    {
        return this with { Type = type };
    }
}
