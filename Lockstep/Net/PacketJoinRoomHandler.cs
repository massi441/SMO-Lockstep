using System.Net;
using System.Text;
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
        if (IsInOtherRoom(packet.Sender, out Player player, out Room takenRoom))
        {
            PlayerInfo info = player.Info;
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

        Result<Player, Error> addResult = room.PlayerHolder.AddPlayer(playerInfo);
        if (addResult.IsFailed)
        {
            return Result<Error>.Failure(addResult.Error!.Value);
        }

        _context.Logger.LogTrace("Player {Name} joined room #{RoomId} with port #{Port}", name, packet.Header.RoomId, addResult.Data!.PortNumber);

        return Result<Error>.Success();
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
