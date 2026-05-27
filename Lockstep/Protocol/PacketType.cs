namespace Lockstep.Protocol;

internal enum PacketType : byte
{
    Connect,
    Disconnect,
    PlayerInput,
    HealthCheck,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
