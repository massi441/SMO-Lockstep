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

    public Result<Error> Handle(Packet packet, Room room)
    {
        Player player = room.PlayerHolder.FindPlayerByHost(packet.Sender)!;

        Result<Error> unregisterResult = room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            _context.Logger.LogError("An error occured while trying to unregister player {Name} from room #{RoomId}", player.Info.Name, room.Id);
            return unregisterResult;
        }

        Span<byte> broadcastBuffer = stackalloc byte[PacketHeader.SizeOf()];
        WriteBroadcast(broadcastBuffer, packet);

        Result<Error> notifyResult = room.Notifier.NotifyOthers(broadcastBuffer, player);
        if (notifyResult.IsSuccess)
        {
            _context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Info.Name, room.Id);
        }

        return notifyResult;
    }

    private static void WriteBroadcast(Span<byte> buffer, Packet packet)
    {
        SpanWriter writer = new SpanWriter(buffer);

        writer.Write(PacketHeader.Magic);
        writer.Write(packet.Header with { Type = PacketType.LeaveRoom });
    }
}
