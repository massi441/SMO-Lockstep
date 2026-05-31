using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerAckJoin
    {
        public required uint Magic;
        public required PacketHeader header;
        public ushort SequenceNumber;
        public required byte SelfPort;
        public required byte OtherPlayersCount;

        public static ushort SizeOf(byte otherPlayersCount)
        {
            return (ushort)(Unsafe.SizeOf<PacketPlayerAckJoin>() + sizeof(byte) * otherPlayersCount);
        }

        public static ushort SizeOfPayload(byte otherPlayersCount)
        {
            // 1-[Port] 1-[OtherPlayersCount] N-[All Ports]
            return (ushort)(sizeof(byte) + sizeof(byte) + sizeof(byte) * otherPlayersCount);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerBroadcastJoin
    {
        public required uint Magic;
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerPort;

        public static ushort SizeOf()
        {
            return (ushort)Unsafe.SizeOf<PacketPlayerBroadcastJoin>();
        }

        public static ushort SizeOfPayload()
        {
            return sizeof(byte);
        }
    }

    public void Handle(Packet packet, Room room)
    {
        if (IsInOtherRoom(packet.Sender, out Player takenRoomPlayer, out Room takenRoom))
        {
            _context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", takenRoomPlayer.Name, takenRoomPlayer.Endpoint.Address, takenRoomPlayer.Endpoint.Port, takenRoom.Id);
            return;
        }

        SpanReader reader = new SpanReader(packet.Payload.Buffer);

        byte nameLength = reader.ReadByte();
        if (!IsValidNameLength(nameLength, packet))
        {
            // TODO: Add log
            _context.Logger.LogWarning("Invalid player name length {Length}", nameLength);
            return;
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = reader.ReadStringUTF8(nameLength),
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
        if (notifyResult.IsSuccess)
        {
            _context.Logger.LogTrace("Player {Name} joined room #{RoomId} with port #{Port}", newPlayer.Name, packet.Header.RoomId, newPlayer.PortNumber);
        }
    }

    private static Result<Error> NotifyRoom(Packet packet, Room room, Player newPlayer)
    {
        byte otherPlayersCount = room.PlayerHolder.OtherPlayerCount;
        byte[] ackBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerAckJoin.SizeOf(otherPlayersCount)); ;

        WriteAck(ackBuffer, packet, room, newPlayer, otherPlayersCount);

        PacketAckBroadcastRequest newPlayerAckRequest = new PacketAckBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            Payload = ackBuffer
        };

        byte[] broadcastBuffer = ArrayPool<byte>.Shared.Rent(PacketPlayerBroadcastJoin.SizeOf());

        WriteBroadcast(broadcastBuffer, packet, newPlayer);
        PacketAckBroadcastRequest broadcastRequest = new PacketAckBroadcastRequest()
        {
            MaxRetries = Config.MaxRetries,
            Payload = broadcastBuffer
        };

        return room.Broadcaster.BroadcastAckExceptWith(room, newPlayer, in newPlayerAckRequest, in broadcastRequest);
    }

    private static void WriteAck(Span<byte> buffer, Packet packet, Room room, Player newPlayer, byte otherPlayersCount)
    {
        ushort payloadSize = PacketPlayerAckJoin.SizeOfPayload(otherPlayersCount);

        PacketPlayerAckJoin ackPacket = new PacketPlayerAckJoin
        {
            Magic = PacketHeader.Magic,
            header = packet.Header.WithSizeType(payloadSize, PacketType.Ack),
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
        PacketPlayerBroadcastJoin broadcastPacket = new PacketPlayerBroadcastJoin
        {
            Magic = PacketHeader.Magic,
            PlayerPort = newPlayer.PortNumber,
            Header = packet.Header.WithSize(PacketPlayerBroadcastJoin.SizeOfPayload())
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

    private static bool IsValidNameLength(int nameLength, Packet Packet)
    {
        return nameLength > 0 && nameLength <= Packet.Payload.Buffer.Length - sizeof(byte);
    }
}
