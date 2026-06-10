using System.Runtime.CompilerServices;
using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

/// <summary>
/// The packet sent to a player that just connected to a room
/// </summary>
internal struct PacketConnectAck : ISerializableStruct
{
    public required PacketHeader Header;
    public ushort SequenceNumber;
    public required Guid SessionId;
    public required byte RoomSize;
    public required byte OtherPlayersCount;
    public required IPlayerHolder PlayerHolder;
    public required Player IgnoredPlayer;

    public ushort FinalizeSize()
    {
        SizeStream stream = new SizeStream();

        stream.Write<PacketHeader>();
        stream.Write<ushort>();
        stream.Write<Guid>();
        stream.Write<byte>();
        stream.Write<byte>();

        foreach (Player player in PlayerHolder.Players)
        {
            if (player == IgnoredPlayer)
            {
                continue;
            }

            PlayerInRoomInfo playerInfo = new PlayerInRoomInfo(player);

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

        foreach (Player player in PlayerHolder.Players)
        {
            if (player == IgnoredPlayer)
            {
                continue;
            }

            PlayerInRoomInfo playerInfo = new PlayerInRoomInfo(player);

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
    public required byte PlayerSlot;
    public required byte PlayerNameLength;
    public required string PlayerName;

    public readonly int Size()
    {
        SizeStream stream = new SizeStream();

        stream.Write<PacketHeader>();
        stream.Write<ushort>();
        stream.Write<byte>();
        stream.Write<byte>();
        stream.WriteString(PlayerName);

        return stream.Size;
    }

    public readonly void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.Write(Header);
        writer.Write(SequenceNumber);
        writer.Write(PlayerSlot);
        writer.Write(PlayerNameLength);
        writer.WriteString(PlayerName);
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
