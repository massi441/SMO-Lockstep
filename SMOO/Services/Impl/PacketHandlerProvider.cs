using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class PacketHandlerProvider : IPacketHandlerProvider
{
    private PacketConnectHandler? _connectHandler;
    private PacketDisconnectHandler? _disconnectHandler;
    private PacketAckHandler? _ackHandler;
    private PacketHealthCheckHandler? _healthCheckHandler;

    public IPacketHandler? GetShared(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => _connectHandler ??= new PacketConnectHandler(context),
            PacketType.Disconnect => _disconnectHandler ??= new PacketDisconnectHandler(context),
            PacketType.HealthCheck => _ackHandler ??= new PacketAckHandler(context),
            PacketType.Ack => _healthCheckHandler ??= new PacketHealthCheckHandler(context),
            _ => null
        };
    }

    public IPacketHandler? GetNew(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(context),
            PacketType.Disconnect => new PacketDisconnectHandler(context),
            PacketType.HealthCheck => new PacketHealthCheckHandler(context),
            PacketType.Ack => new PacketAckHandler(context),
            _ => null
        };
    }
}
