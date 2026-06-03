namespace SMOO.Protocol;

internal enum PacketType : byte
{
    RequestJoinRoom,
    PlayerJoinRoomSelf,
    PlayerJoinRoomBroadcast,
    PlayerLeaveRoom,
    PlayerInput,
    HealthCheck,
    Ping,
    Ack,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
