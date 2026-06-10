namespace SMOO.Protocol;

internal enum PacketType : byte
{
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

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
