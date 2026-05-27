namespace Lockstep.Protocol;

internal enum PacketType : byte
{
    Connect = 0,
    Disconnect,
    PlayerInput,
    HealthCheck,

    /// <summary>
    /// A reserved packet type for server side validation
    /// </summary>
    Invalid
}
