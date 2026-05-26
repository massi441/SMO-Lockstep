using System.Runtime.CompilerServices;

namespace Lockstep.Protocol;

internal struct PacketHeader
{
    public PacketType Type;
    public byte Version;
    public short PayloadSize;

    public static int SizeOf()
    {
        return Unsafe.SizeOf<PacketHeader>();
    }
}
