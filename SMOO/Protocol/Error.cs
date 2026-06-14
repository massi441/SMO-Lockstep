namespace SMOO.Protocol;

internal enum Error
{
    // Packet header
    InvalidMagic,
    InvalidHeaderSize,
    InvalidPacketType,
    InvalidVersion,

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
    OperationFailed,
}
