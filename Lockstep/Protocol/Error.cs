namespace Lockstep.Protocol;

internal enum Error
{
    // Packet header
    InvalidMagic,
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,
    InvalidPayloadSize,

    // Packet Handling
    EmptyPayload,
    NoPacketHandler,
    InvalidNameLength,

    // Packet Sending
    NotSent,
    PendingPacketStoreFull,

    // Room
    RoomNotFound,
    RoomFull,
    PlayerAlreadyInRoom,
    IllegalRoomAccess,

    // Generic
    JobWriteFailed,
    OperationFailed,
}
