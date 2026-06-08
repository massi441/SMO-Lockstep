using SMOO.Handle;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class PacketHandlerProvider : IPacketHandlerProvider
{
    private PacketConnectHandler? _connectHandler;
    private PacketConnectSynAckHandler? _connectSynAckHandler;
    private PacketDisconnectHandler? _disconnectHandler;
    private PacketAckHandler? _ackHandler;
    private PacketHealthCheckHandler? _healthCheckHandler;
    private PacketChatMessageHandler? _chatHandler;

    public IPacketHandler? GetShared(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => _connectHandler ??= new PacketConnectHandler(context),
            PacketType.ConnectSynAck => _connectSynAckHandler ??= new PacketConnectSynAckHandler(context),
            PacketType.Disconnect => _disconnectHandler ??= new PacketDisconnectHandler(context),
            PacketType.HealthCheck => _healthCheckHandler ??= new PacketHealthCheckHandler(context),
            PacketType.Ack => _ackHandler ??= new PacketAckHandler(context),
            PacketType.ChatMessage => _chatHandler ??= new PacketChatMessageHandler(context),
            _ => null
        };
    }

    public IPacketHandler? GetNew(PacketType packetType, ServerContext context)
    {
        return packetType switch
        {
            PacketType.Connect => new PacketConnectHandler(context),
            PacketType.ConnectSynAck => new PacketConnectSynAckHandler(context),
            PacketType.Disconnect => new PacketDisconnectHandler(context),
            PacketType.HealthCheck => new PacketHealthCheckHandler(context),
            PacketType.Ack => new PacketAckHandler(context),
            PacketType.ChatMessage => new PacketChatMessageHandler(context),
            _ => null
        };
    }
}
