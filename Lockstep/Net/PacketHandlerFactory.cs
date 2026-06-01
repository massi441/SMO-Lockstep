using Lockstep.Protocol;
using Lockstep.Server;

namespace Lockstep.Net;

internal static class PacketHandlerFactory
{
    public static IPacketHandler? CreateHandler(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.JoinRoom => new PacketJoinRoomHandler(context),
            PacketType.LeaveRoom => new PacketLeaveRoomHandler(context),
            PacketType.PlayerInput => new PacketPlayerInputHandler(context),
            PacketType.HealthCheck => new PacketHealthCheckHandler(context),
            PacketType.Ack => new PacketAckHandler(context),
            _ => null
        };
    }
}
