namespace Lockstep.Protocol;

internal enum PacketType : byte
{
    Connect = 0,
    Disconnect,
    PlayerInput,
    HealthCheck,
    Invalid
}
