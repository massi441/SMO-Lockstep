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

        Result<Error> disconnectResult = _context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsFailed)
        {
            _context.Logger.LogError("Unable to disconnect {PlayerName} in room #{RoomId}", player.Name, room.Id);
        }

        _context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Name, room.Id);
    }
}
