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

    public Result<Error> Disconnect(Player player)
    {
        Span<byte> broadcastBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerLeaveRoom.SizeOf());

        PacketHeader header = new PacketHeader()
        {
            Type = PacketType.LeaveRoom,
            Flags = (byte)PacketFlags.None,
            Version = Config.Version,
            RoomId = player.Room.Id,
            PayloadSize = PacketPlayerLeaveRoom.SizeOfPayload()
        };

        PacketPlayerLeaveRoom leavePacket = new PacketPlayerLeaveRoom()
        {
            Magic = PacketHeader.Magic,
            Header = header,
            PlayerPort = player.PortNumber
        };

        MemoryMarshal.Write(broadcastBuffer, leavePacket);

        return player.Room.Broadcaster.BroadcastExcept(player.Room, player, broadcastBuffer);
    }
}
