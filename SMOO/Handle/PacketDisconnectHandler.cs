using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Handle;

internal class PacketDisconnectHandler : IPacketHandler
{
    private readonly ServerContext _context;
    public uint MinPayloadSize => 0;

    public PacketDisconnectHandler(ServerContext context)
    {
        _context = context;
    }

    public void Handle(ParsedPacket packet, Room room)
    {
        Player? player = packet.SenderPlayer;

        if (player == null)
        {
            packet.RentedBuffer.Return();
            _context.Logger.LogWarning("Player was null in PacketDisconnect handler");
            return;
        }

        Result<Error> disconnectResult = _context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsFailed)
        {
            packet.RentedBuffer.Return();
            _context.Logger.LogError("Unable to disconnect {PlayerName} in room #{RoomId}", player.Name, room.Id);
            return;
        }

        packet.RentedBuffer.Return();

        _context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Name, room.Id);
    }
}
