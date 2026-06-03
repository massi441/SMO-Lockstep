using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Handle;

internal class PacketJoinRoomHandler : IPacketHandler
{
    private readonly ServerContext _context;

    /// <summary>
    /// Requires at least one UInt16 for the length of the Player's name
    /// </summary>
    public uint MinPayloadSize => 2;

    public PacketJoinRoomHandler(ServerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// The packet sent to a player requesting to join a room
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerJoinRoomSelfAck
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte SelfPort;
        public required byte OtherPlayersCount;

        public static ushort SizeOf(byte otherPlayersCount)
        {
            return (ushort)(Unsafe.SizeOf<PacketPlayerJoinRoomSelfAck>() + sizeof(byte) * otherPlayersCount);
        }

        public static ushort SizeOfPayload(byte otherPlayersCount)
        {
            // 1-[Port] 1-[OtherPlayersCount] N-[All Ports]
            return (ushort)(sizeof(byte) + sizeof(byte) + sizeof(byte) * otherPlayersCount);
        }
    }

    /// <summary>
    /// The packet sent to notify a room that a player has joined
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerJoinRoomAck
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerPort;

        public static ushort SizeOf()
        {
            return (ushort)Unsafe.SizeOf<PacketPlayerJoinRoomAck>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private readonly ref struct PlayerJoinRoomPayload
    {
        private readonly ReadOnlySpan<byte> _buffer;

        /// <summary>
        /// The length of the player's name, starting at offset 0x0
        /// </summary>
        public readonly byte NameLength => _buffer[0];

        /// <summary>
        /// The name of the player, starting at offset 0x1
        /// </summary>
        public readonly string Name => Encoding.UTF8.GetString(_buffer.Slice(0x1, NameLength));
        
        public PlayerJoinRoomPayload(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
        }
    }

    public void Handle(Packet packet, Room room)
    {
        if (IsInOtherRoom(packet.Sender, out Player takenRoomPlayer, out Room takenRoom))
        {
            _context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", takenRoomPlayer.Name, takenRoomPlayer.Endpoint.Address, takenRoomPlayer.Endpoint.Port, takenRoom.Id);
            return;
        }

        PlayerJoinRoomPayload joinPayload = new PlayerJoinRoomPayload(packet.Payload);

        byte nameLength = joinPayload.NameLength;

        if (!IsValidNameLength(nameLength))
        {
            _context.Logger.LogWarning("Invalid player name length {Length}", nameLength);
            return;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = joinPayload.Name,
            Room = room,
        };

        Result<Player, Error> addResult = room.PlayerHolder.RegisterPlayer(playerInfo);
        if (addResult.IsFailed)
        {
            _context.Logger.LogError("Failed to register {PlayerName} in Room #{RoomId}", playerInfo.Name, room.Id);
            return;
        }

        Player newPlayer = addResult.Data!;
        
        Result<Error> notifyResult = NotifyRoom(packet, room, newPlayer);
        if (notifyResult.IsFailed)
        {
            _context.Logger.LogError("Failed to notify {PlayerName} joining Room #{RoomId}", playerInfo.Name, room.Id);
            return;
        }

        _context.Logger.LogTrace("Player {Name} joined room #{RoomId} with port #{Port}", newPlayer.Name, packet.Header.RoomId, newPlayer.PortNumber);
    }

    private static Result<Error> NotifyRoom(Packet packet, Room room, Player newPlayer)
    {
        byte otherPlayersCount = room.PlayerHolder.OtherPlayerCount;
        byte[] ackBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerJoinRoomSelfAck.SizeOf(otherPlayersCount)); ;

        WriteSelfAck(ackBuffer, packet, room, newPlayer, otherPlayersCount);

        PacketAckBroadcastRequest newPlayerAckRequest = new PacketAckBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            Payload = ackBuffer
        };

        byte[] broadcastBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerJoinRoomAck.SizeOf());

        WriteBroadcast(broadcastBuffer, packet, newPlayer);
        PacketAckBroadcastRequest broadcastRequest = new PacketAckBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            Payload = broadcastBuffer
        };

        return room.Broadcaster.BroadcastAckExceptWith(room, newPlayer, in newPlayerAckRequest, in broadcastRequest);
    }

    private static void WriteSelfAck(Span<byte> buffer, Packet packet, Room room, Player newPlayer, byte otherPlayersCount)
    {
        ushort payloadSize = PacketPlayerJoinRoomSelfAck.SizeOfPayload(otherPlayersCount);

        PacketPlayerJoinRoomSelfAck ackPacket = new PacketPlayerJoinRoomSelfAck
        {
            Header = packet.Header.WithSizeType(payloadSize, PacketType.AckConnect),
            SelfPort = newPlayer.PortNumber,
            OtherPlayersCount = otherPlayersCount
        };

        SpanWriter writer = new SpanWriter(buffer);

        writer.Write(ackPacket);

        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player != newPlayer)
            {
                writer.Write(player.PortNumber);
            }
        }
    }

    private static void WriteBroadcast(Span<byte> buffer, Packet packet, Player newPlayer)
    {
        PacketPlayerJoinRoomAck broadcastPacket = new PacketPlayerJoinRoomAck
        {
            PlayerPort = newPlayer.PortNumber,
            Header = packet.Header.WithSizeType(PacketPlayerJoinRoomAck.SizeOfPayload(), PacketType.PlayerJoinRoomBroadcast)
        };

        MemoryMarshal.Write(buffer, broadcastPacket);
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
