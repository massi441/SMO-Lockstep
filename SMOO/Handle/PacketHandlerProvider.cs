using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal readonly unsafe struct PacketHandler
{
    public readonly ushort MinPayloadSize;
    public readonly delegate*<ParsedPacket, Room, ServerContext, void> Handler;

    public PacketHandler(ushort minPayloadSize, delegate*<ParsedPacket, Room, ServerContext, void> handler)
    {
        MinPayloadSize = minPayloadSize;
        Handler = handler;
    }
}

internal static unsafe class PacketHandlerProvider
{
    private static readonly PacketHandler?[] Handlers =
    [
        new PacketHandler(PacketConnectHandler.MinPayloadSize, &PacketConnectHandler.Handle),
        null,
        new PacketHandler(PacketConnectSynAckHandler.MinPayloadSize, &PacketConnectSynAckHandler.Handle),
        new PacketHandler(PacketDisconnectHandler.MinPayloadSize, &PacketDisconnectHandler.Handle),
        null,
        null,
        new PacketHandler(PacketHealthCheckHandler.MinPayloadSize, &PacketHealthCheckHandler.Handle),
        null,
        new PacketHandler(PacketAckHandler.MinPayloadSize, &PacketAckHandler.Handle),
        new PacketHandler(PacketChatMessageHandler.MinPayloadSize, &PacketChatMessageHandler.Handle),
        null,
    ];

    public static PacketHandler? GetHandler(PacketType type)
    {
        return Handlers[(byte)type];
    }
}
