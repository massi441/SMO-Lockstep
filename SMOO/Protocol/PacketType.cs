namespace SMOO.Protocol;

internal enum PacketType : byte
{
    Connect,
    ConnectAck,
    ConnectSynAck,
    Disconnect,
    PlayerJoinRoomBroadcast,
    PlayerInput,
    HealthCheck,
    Ping,
    Ack,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
