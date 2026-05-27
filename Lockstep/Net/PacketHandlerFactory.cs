using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketHandlerFactory
{
    public static IPacketHandler? CreateHandler(PacketType packetType)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(Logger.Get()),
            PacketType.Disconnect => new PacketDisconnectHandler(Logger.Get()),
            PacketType.PlayerInput => new PacketPlayerInputHandler(Logger.Get()),
            PacketType.HealthCheck => new PacketHealthCheckHandler(Logger.Get()),
            _ => null
        };
    }
}