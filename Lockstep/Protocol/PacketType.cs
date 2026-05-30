namespace Lockstep.Protocol;

internal enum PacketType : byte
{
    Connect,
    Disconnect,
    JoinRoom,
    PlayerInput,
    HealthCheck,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
