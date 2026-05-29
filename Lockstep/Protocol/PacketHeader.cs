using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lockstep.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PacketHeader
{
    public PacketType Type;
    public byte Version;
    public short PayloadSize;

    public const uint Magic = 0x534D4F4C; // "SMOL"

    public static int SizeOfSender()
    {
        return sizeof(uint) + Unsafe.SizeOf<PacketHeader>();
    }
}
