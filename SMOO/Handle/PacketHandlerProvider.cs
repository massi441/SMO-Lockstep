using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal static class PacketHandlerProvider
{
    public static IPacketHandler? CreateHandler(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.RequestJoinRoom => new PacketJoinRoomHandler(context),
            PacketType.PlayerLeaveRoom => new PacketLeaveRoomHandler(context),
            PacketType.PlayerInput => new PacketPlayerInputHandler(context),
            PacketType.HealthCheck => new PacketHealthCheckHandler(context),
            PacketType.Ack => new PacketAckHandler(context),
            _ => null
        };
    }
}
