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

    // Must provide size of the player's name
    public uint MinPayloadSize => 2;

    public PacketJoinRoomHandler(ServerContext context)
    {
        _context = context;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlayerBroadcastJoinPacket
    {
        public uint Magic;
        public PacketHeader header;
        public byte PlayerPort;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PlayerBroadcastJoinPacket>();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlayerAckJoinPacket
    {
        public uint Magic;
        public PacketHeader header;
        public byte SelfPort;
        public byte OtherPlayersCount;

        public static int SizeOf(byte otherPlayersCount)
        {
            return Unsafe.SizeOf<PlayerAckJoinPacket>() + sizeof(byte) * otherPlayersCount;
        }
    }

    public Result<Error> Handle(Packet packet, Room room)
    {
        if (IsInOtherRoom(packet.Sender, out Player takenRoomPlayer, out Room takenRoom))
        {
            PlayerInfo info = takenRoomPlayer.Info;
            _context.Logger.LogWarning("Player {Name} ({Address}:{Port}) is already in room {RoomId}", info.Name, info.Endpoint.Address, info.Endpoint.Port, takenRoom.Id);
            return Result<Error>.Failure(Error.PlayerAlreadyInRoom);
        }

        SpanReader reader = new SpanReader(packet.Payload.Buffer);

        byte nameLength = reader.ReadByte();
        if (!IsValidNameLength(nameLength, packet))
        {
            return Result<Error>.Failure(Error.InvalidNameLength);
        }

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = reader.ReadStringUTF8(nameLength)
        };

        Result<Player, Error> addResult = room.PlayerHolder.RegisterPlayer(playerInfo);
        if (addResult.IsFailed)
        {
            return Result<Error>.Failure(addResult.Error!.Value);
        }

        return NotifyRoom(packet, room, addResult.Data!);
    }

    private Result<Error> NotifyRoom(Packet packet, Room room, Player newPlayer)
    {
        Span<byte> broadcastBuffer = stackalloc byte[PlayerBroadcastJoinPacket.SizeOf()];

        WriteBroadcast(broadcastBuffer, packet, newPlayer);

        byte otherPlayersCount = room.PlayerHolder.OtherPlayerCount;
        Span<byte> ackBuffer = stackalloc byte[PlayerAckJoinPacket.SizeOf(otherPlayersCount)];

        WriteAck(ackBuffer, packet, room, newPlayer, otherPlayersCount);

        Result<Error> notifyResult = room.Notifier.NotifyOthers(broadcastBuffer, newPlayer, ackBuffer);
        if (notifyResult.IsSuccess)
        {
            _context.Logger.LogTrace("Player {Name} joined room #{RoomId} with port #{Port}", newPlayer.Info.Name, packet.Header.RoomId, newPlayer.PortNumber);
        }

        return notifyResult;
    }

    private static void WriteBroadcast(Span<byte> buffer, Packet packet, Player newPlayer)
    {
        PlayerBroadcastJoinPacket broadcastPacket = new PlayerBroadcastJoinPacket
        {
            Magic = PacketHeader.Magic,
            PlayerPort = newPlayer.PortNumber,
            header = packet.Header
        };

        MemoryMarshal.Write(buffer, broadcastPacket);
    }

    private static void WriteAck(Span<byte> buffer, Packet packet, Room room, Player newPlayer, byte otherPlayersCount)
    {
        PlayerAckJoinPacket ackPacket = new PlayerAckJoinPacket
        {
            Magic = PacketHeader.Magic,
            header = packet.Header.WithType(PacketType.Ack),
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
