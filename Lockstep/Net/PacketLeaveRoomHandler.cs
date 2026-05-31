using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketLeaveRoomHandler : IPacketHandler
{
    private readonly ServerContext _context;
    public uint MinPayloadSize => 0;

    public PacketLeaveRoomHandler(ServerContext context)
    {
        _context = context;
    }

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

    public Result<Error> Handle(Packet packet, Room room)
    {
        Player player = room.PlayerHolder.FindPlayerByHost(packet.Sender)!;

        Result<Error> unregisterResult = room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            _context.Logger.LogError("An error occured while trying to unregister player {Name} from room #{RoomId}", player.Info.Name, room.Id);
            return unregisterResult;
        }

        Span<byte> broadcastBuffer = stackalloc byte[PacketPlayerLeaveRoom.SizeOf()];
        WriteBroadcast(broadcastBuffer, packet, player);

        Result<Error> notifyResult = room.Notifier.NotifyOthers(broadcastBuffer, player);
        if (notifyResult.IsSuccess)
        {
            _context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Info.Name, room.Id);
        }

        return notifyResult;
    }

    private static void WriteBroadcast(Span<byte> buffer, Packet packet, Player player)
    {
        PacketPlayerLeaveRoom leavePacket = new PacketPlayerLeaveRoom()
        {
            Magic = PacketHeader.Magic,
            Header = packet.Header.WithSizeType(PacketPlayerLeaveRoom.SizeOfPayload(), PacketType.LeaveRoom),
            PlayerPort = player.PortNumber
        };

        MemoryMarshal.Write(buffer, leavePacket);
    }
}
