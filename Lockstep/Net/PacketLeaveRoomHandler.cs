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

    public void Handle(Packet packet, Room room)
    {
        Player? player = room.PlayerHolder.FindPlayerByHost(packet.Sender)!;

        Result<Error> unregisterResult = room.PlayerHolder.UnregisterPlayer(player);
        if (unregisterResult.IsFailed)
        {
            _context.Logger.LogError("An error occured while trying to unregister player {Name} from room #{RoomId}", player.Name, room.Id);
            return;
        }

        Result<Error> disconnectResult = _context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsSuccess)
        {
            _context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Name, room.Id);
        }
    }
}
