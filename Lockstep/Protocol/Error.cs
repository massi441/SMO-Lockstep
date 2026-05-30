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
    InvalidNameLength,

    // Packet Sending
    NotSent,

    // Room
    PlayerAlreadyInRoom,
    IllegalRoomAccess,
    RoomFull,

    // Generic
    JobWriteFailed,
    OperationFailed,
}
