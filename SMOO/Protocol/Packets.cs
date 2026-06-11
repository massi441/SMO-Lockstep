using System.Runtime.CompilerServices;
using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

/// <summary>
/// The packet sent to a player that just connected to a room
/// </summary>
internal ref struct PacketConnectAck : ISerializableStruct
{
    public required PacketHeader Header;
    public ushort SequenceNumber;
    public required Guid SessionId;
    public required byte RoomSize;
    public required byte OtherPlayersCount;
    public required ReadOnlySpan<PlayerInRoomInfo> PlayerInfos;

    public PacketConnectAck()
    {
        SequenceNumber = 0;
    }

    public ushort FinalizeSize()
    {
        SizeStream stream = new SizeStream();

        stream.Write<PacketHeader>();
        stream.Write<ushort>();
        stream.Write<Guid>();
        stream.Write<byte>();
        stream.Write<byte>();

        foreach (PlayerInRoomInfo playerInfo in PlayerInfos)
        {
            stream.WriteBytes(playerInfo.Size());
        }

        ushort fullsize = stream.Size;

        Header.PayloadSize = (ushort)(fullsize - Unsafe.SizeOf<PacketHeader>());

        return fullsize;
    }

    public readonly void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.Write(Header);
        writer.Write(SequenceNumber);
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
    public required PacketHeader Header;
    public ushort SequenceNumber;
    public required PlayerInRoomInfo PlayerRoomInfo;

    public PacketPlayerJoinRoom()
    {
        SequenceNumber = 0;
        PlayerRoomInfo = new PlayerInRoomInfo();
    }

    public int FinalizeSize()
    {
        SizeStream stream = new SizeStream();

        stream.Write<PacketHeader>();
        stream.Write<ushort>();
        stream.WriteBytes(PlayerRoomInfo.Size());

        ushort fullsize = stream.Size;

        Header.PayloadSize = (ushort)(fullsize - Unsafe.SizeOf<PacketHeader>());

        return stream.Size;
    }

    public readonly void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.Write(Header);
        writer.Write(SequenceNumber);

        PlayerRoomInfo.Serialize(ref writer);
    }
}

/// <summary>
/// The packet broadcasted to a room when a player sends a chat message
/// </summary>
internal struct PacketChatMessage : ISerializableStruct
{
    public required PacketHeader Header;
    public ushort SequenceNumber;
    public required byte PlayerSlot;
    public required ushort MessageLength;
    public required string Message;

    public PacketChatMessage()
    {
        SequenceNumber = 0;
    }

    public readonly int Size()
    {
        SizeStream stream = new SizeStream();

        stream.Write<PacketHeader>();
        stream.Write<ushort>();
        stream.Write<byte>();
        stream.Write<ushort>();
        stream.WriteString(Message);

        return stream.Size;
    }

    public readonly void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.Write(Header);
        writer.Skip(sizeof(ushort));
        writer.Write(PlayerSlot);
        writer.Write(MessageLength);
        writer.WriteString(Message);
    }
}
