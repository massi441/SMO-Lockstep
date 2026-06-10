using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Handle;

internal class PacketDisconnectHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        Player? player = packet.SenderPlayer;

        if (player == null)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogWarning("Player was null in PacketDisconnect handler");
            return;
        }

        Result<Error> disconnectResult = context.PlayerDisconnector.Disconnect(player);
        if (disconnectResult.IsFailed)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogError("Unable to disconnect {PlayerName} in room #{RoomId}", player.Name, room.Id);
            return;
        }

        packet.RentedBuffer.Return();

        context.Logger.LogWarning("Player {Name} left room {RoomId}", player.Name, room.Id);
    }
}
