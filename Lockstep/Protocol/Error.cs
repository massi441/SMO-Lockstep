namespace Lockstep.Protocol;

internal enum Error
{
    // Packet header
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,
    InvalidPayloadSize,

    // Packet
    EmptyPayload
}
