using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Util;

namespace SMOO.Protocol;

/// <summary>
/// The packet sent to a player that just connected to a room
/// </summary>
internal ref struct PacketConnectAck : ISerializableStruct
{
    public required PacketHeader Header;
    public required Guid SessionId;
    public required byte RoomSize;
    public required byte OtherPlayersCount;
    public required PlayerInRoomInfoEnumerator PlayerInfos;

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
        writer.Write(SessionId);
        writer.Write(RoomSize);
        writer.Write(OtherPlayersCount);

        foreach (PlayerInRoomInfo playerInfo in PlayerInfos)
        {
            playerInfo.Serialize(ref writer);
        }
    }
}

/// <summary>
/// The packet sent to a room, to notify that a new player has joined
/// </summary>
internal struct PacketPlayerJoinRoom : ISerializableStruct
{
    [RequiredField]
    public required PacketHeader Header;

    [RequiredField]
    public required PlayerInRoomInfo PlayerRoomInfo;

    public PacketPlayerJoinRoom()
    {
        PlayerRoomInfo = new PlayerInRoomInfo();
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);

        PlayerRoomInfo.Serialize(ref writer);
    }
}

/// <summary>
/// The packet broadcasted to a room when a player sends a chat message
/// </summary>
internal struct PacketChatMessage : ISerializableStruct
{
    [RequiredField]
    public required PacketHeader Header;

    [RequiredField]
    public required byte PlayerSlot;

    [DynamicField(MaxSize = Config.MaxChatMessageLength)]
    public required StreamStringView<ushort> Message;

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Header);
        writer.Write(PlayerSlot);

        Message.Serialize(ref writer);
    }
}
