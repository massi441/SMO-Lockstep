using System.Net;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketJoinRoomHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public uint MinPayloadSize => 2;

    public PacketJoinRoomHandler(ServerContext context)
    {
        _context = context;
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

        string name = reader.ReadStringUTF8(nameLength);

        PlayerInfo playerInfo = new PlayerInfo()
        {
            Endpoint = packet.Sender,
            Name = name
        };

        Result<Player, Error> addResult = room.PlayerHolder.RegisterPlayer(playerInfo);
        if (addResult.IsFailed)
        {
            return Result<Error>.Failure(addResult.Error!.Value);
        }

        Span<byte> broadcastBuffer = stackalloc byte[PacketHeader.SizeOf()];
        WriteBroadcast(broadcastBuffer, packet);

        Span<byte> senderBuffer = stackalloc byte[PacketHeader.SizeOf()];
        WriteSender(senderBuffer, packet);

        Player newPlayer = addResult.Data!;

        Result<Error> notifyResult = room.Notifier.NotifyOthers(broadcastBuffer, newPlayer, senderBuffer);
        if (notifyResult.IsSuccess)
        {
            _context.Logger.LogTrace("Player {Name} joined room #{RoomId} with port #{Port}", name, packet.Header.RoomId, newPlayer.PortNumber);
        }

        return notifyResult;
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

    private static void WriteSender(Span<byte> buffer, Packet packet)
    {
        SpanWriter writer = new SpanWriter(buffer);

        writer.Write(PacketHeader.Magic);
        writer.Write(packet.Header with { Type = PacketType.Ack });
    }

    private static void WriteBroadcast(Span<byte> buffer, Packet packet)
    {
        SpanWriter writer = new SpanWriter(buffer);

        writer.Write(PacketHeader.Magic);
        writer.Write(packet.Header with { Type = PacketType.LeaveRoom });
    }
}
