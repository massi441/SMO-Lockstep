using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal class PlayerDisconnector : IPlayerDisconnector
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerLeaveRoom
    {
        public uint Magic;
        public PacketHeader Header;
        public byte PlayerPort;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PacketPlayerLeaveRoom>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }
    }

    public Result<Error> Disconnect(Player player, Packet originalPacket, Room room)
    {
        Span<byte> broadcastBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerLeaveRoom.SizeOf());
        PacketPlayerLeaveRoom leavePacket = new PacketPlayerLeaveRoom()
        {
            Magic = PacketHeader.Magic,
            Header = originalPacket.Header.WithSizeType(PacketPlayerLeaveRoom.SizeOfPayload(), PacketType.LeaveRoom),
            PlayerPort = player.PortNumber
        };

        MemoryMarshal.Write(broadcastBuffer, leavePacket);

        return room.Broadcaster.BroadcastExcept(broadcastBuffer, player);
    }
}
