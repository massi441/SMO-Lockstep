namespace Lockstep.Protocol;

internal enum Error
{
    // Packet header
    InvalidMagic,
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,
    InvalidPayloadSize,
    InvalidRoomId,

    // Packet Handling
    EmptyPayload,
    NoPacketHandler,

    // Packet Sending
    NotSent,

    // Generic
    JobWriteFailed
}
