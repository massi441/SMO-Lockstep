using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketHandlerFactory
{
    public static IPacketHandler? CreateHandler(PacketType packetType, ServiceProvider serviceProvider)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(serviceProvider.PacketSender, serviceProvider.Logger),
            PacketType.Disconnect => new PacketDisconnectHandler(serviceProvider.Logger),
            PacketType.PlayerInput => new PacketPlayerInputHandler(serviceProvider.Logger),
            PacketType.HealthCheck => new PacketHealthCheckHandler(serviceProvider.Logger),
            _ => null
        };
    }
}
