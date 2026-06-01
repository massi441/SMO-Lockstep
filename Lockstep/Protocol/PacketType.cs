namespace Lockstep.Protocol;

internal enum PacketType : byte
{
    JoinRoom,
    LeaveRoom,
    PlayerInput,
    HealthCheck,
    Ping,
    Ack,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
