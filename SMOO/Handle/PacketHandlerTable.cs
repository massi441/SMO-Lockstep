using System.Diagnostics;
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

internal static unsafe class PacketHandlerTable
{
    private static readonly PacketHandler DefaultHandler        = new PacketHandler(PacketDefaultHandler.MinPayloadSize, &PacketDefaultHandler.Handle);
    private static readonly PacketHandler Connect               = new PacketHandler(PacketConnectHandler.MinPayloadSize, &PacketConnectHandler.Handle);
    private static readonly PacketHandler ConnectAck            = DefaultHandler;
    private static readonly PacketHandler ConnectSynAck         = new PacketHandler(PacketConnectSynAckHandler.MinPayloadSize, &PacketConnectSynAckHandler.Handle);
    private static readonly PacketHandler Disconnect            = new PacketHandler(PacketDisconnectHandler.MinPayloadSize, &PacketDisconnectHandler.Handle);
    private static readonly PacketHandler PlayerJoinRoom        = DefaultHandler;
    private static readonly PacketHandler HealthCheck           = new PacketHandler(PacketHealthCheckHandler.MinPayloadSize, &PacketHealthCheckHandler.Handle);
    private static readonly PacketHandler Ping                  = DefaultHandler;
    private static readonly PacketHandler Ack                   = new PacketHandler(PacketAckHandler.MinPayloadSize, &PacketAckHandler.Handle);
    private static readonly PacketHandler ChatMessage           = DefaultHandler;
    private static readonly PacketHandler ChatMessageRequest    = new PacketHandler(PacketChatMessageHandler.MinPayloadSize, &PacketChatMessageHandler.Handle);
    private static readonly PacketHandler Event                 = new PacketHandler(PacketEventHandler.MinPayloadSize, &PacketEventHandler.Handle);

    private static readonly PacketHandler[] Handlers =
    [
        Connect,
        ConnectAck,
        ConnectSynAck,
        Disconnect,
        PlayerJoinRoom,
        HealthCheck,
        Ping,
        Ack,
        ChatMessage,
        ChatMessageRequest,
        Event,
    ];

    static PacketHandlerTable()
    {
        Debug.Assert(Handlers.Length == (byte)PacketType.Invalid, "Handlers table is out of sync with PacketType enum");
    }

    public static PacketHandler GetHandler(PacketType type)
    {
        byte index = (byte)type;

        if (index < Handlers.Length)
        {
            return Handlers[index];
        }

        return DefaultHandler;
    }
}
