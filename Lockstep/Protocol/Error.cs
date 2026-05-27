namespace Lockstep.Protocol;

internal enum Error
{
    // Packet header
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,
    InvalidPayloadSize,

    // Packet Handling
    EmptyPayload,
    NoPacketHandler
}
