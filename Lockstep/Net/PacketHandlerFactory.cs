using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketHandlerFactory
{
    public static IPacketHandler? CreateHandler(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(context),
            _ => null
        };
    }
}
