using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly ServerContext _context;

    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public uint MinPayloadSize => 2;

    public PacketConnectHandler(ServerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// The payload sent by a client who requests to connect to a room
    /// </summary>
    private struct PacketConnectPayload : IDeserializableStruct
    {
        /// <summary>
        /// The length of the player's name, starting at offset 0x0
        /// </summary>
        public byte NameLength { get; private set; }

        /// <summary>
        /// The name of the player, starting at offset 0x1
        /// </summary>
        public string Name { get; private set; }

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            NameLength = source[0];
            Name = Encoding.UTF8.GetString(source.Slice(0x1, NameLength));
        }
    }

    /// <summary>
    /// The packet sent to a player that just connected to a room
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketConnectAck : ISerializableStruct
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte RoomSize;
        public required Guid SessionId;

        public static ushort SizeOf()
        {
            return (ushort)Unsafe.SizeOf<PacketConnectAck>();
        }

        public readonly void Serialize(Span<byte> destination)
        {
            MemoryMarshal.Write(destination, this);
        }
    }

    public void Handle(Packet packet, Room room, Player? player)
    {
        if (IsInOtherRoom(packet.Sender, out player, out Room takenRoom))
        {
            _context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", player.Name, player.Endpoint.Address, player.Endpoint.Port, takenRoom.Id);
            return;
        }

        PacketConnectPayload connectPayload = PacketSerializer.Deserialize<PacketConnectPayload>(packet.Payload);

        if (!IsValidNameLength(connectPayload.NameLength))
        {
            _context.Logger.LogWarning("Invalid player name length {Length}", connectPayload.NameLength);
            return;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = connectPayload.Name,
            Room = room,
        };

        Result<Player, Error> newPlayerResult = room.PlayerHolder.RegisterPlayer(in playerInfo);
        if (newPlayerResult.IsFailed)
        {
            _context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            return;
        }

        Player newPlayer = newPlayerResult.Data!;

        if (!AckConnect(newPlayer, packet, room))
        {
            _context.Logger.LogError("Failed to upload connect ACK packet, new player will be ignored");
            return;
        }

        _context.Logger.LogTrace("Player {Name} joined Room #{RoomId}, waiting for a confirmation...", newPlayer.Name, packet.Header.RoomId);
    }

    private bool AckConnect(Player newPlayer, Packet packet, Room room)
    {
        RentedBuffer ackBuffer = MemoryUtil.Rent<PacketConnectAck>();

        PacketConnectAck ackPacket = new PacketConnectAck()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketConnectAck>(), PacketType.ConnectAck),
            RoomSize = room.PlayerHolder.MaxSize,
            SessionId = newPlayer.Id.SessionId
        };

        ackPacket.Serialize(ackBuffer.Span);

        ReliablePacketRequest ackRequest = new ReliablePacketRequest()
        {
            Receiver = newPlayer,
            RentedPayload = ackBuffer,
            MaxRetries = Config.MaxRetries
        };

        Result<Error> uploadResult = room.Broadcaster.ReliablePacketStore.UploadPacket(ackRequest);
        if (uploadResult.IsFailed)
        {
            return false;
        }

        _context.PacketSender.Send(newPlayer.Endpoint, ackBuffer.Span);

        return true;
    }

    private bool IsInOtherRoom(IPEndPoint sender, out Player player, out Room takenRoom)
    {
        foreach (Room room in _context.RoomHolder.GetRooms())
        {
            Player? p = room.PlayerHolder.FindPlayerByHost(sender);
            if (p != null)
            {
                player = p;
                takenRoom = room;
                return true;
            }
        }

        player = null!;
        takenRoom = null!;

        return false;
    }

    private static bool IsValidNameLength(int nameLength)
    {
        return nameLength > 0 && nameLength <= Config.MaxPlayerNameLength;
    }
}
