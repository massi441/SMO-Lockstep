using Lockstep.Protocol;
using Lockstep.Server;

namespace Lockstep.Net;

internal static class PacketHandlerFactory
{
    public static IPacketHandler? CreateHandler(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(context),
            PacketType.JoinRoom => new PacketJoinRoomHandler(context),
            PacketType.PlayerInput => new PacketPlayerInputHandler(context),
            _ => null
        };
    }
}
