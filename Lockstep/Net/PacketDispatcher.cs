using System.Net;
using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Payload packet, ServerContext context)
    {
        Result<PacketHeader, Error> headerResult = PacketParser.ParseHeader(packet.Buffer.Span);

        if (headerResult.IsSuccess)
        {
            PacketHeader header = headerResult.Data;

            Room? room = context.RoomHolder.GetRoom(header.RoomId);
            if (room == null)
            {
                return Result<Error>.Failure(Error.InvalidRoomId);
            }

            if (!IsAllowedInRoom(packet.Sender, room, ref header))
            {
                return Result<Error>.Failure(Error.IllegalRoomAccess);
            }

            Packet roomPacket = new Packet()
            {
                Header = header,
                Payload = new Payload(packet, PacketHeader.SizeOf()),
            };

            if (!room.Packets.Writer.TryWrite(roomPacket))
            {
                return Result<Error>.Failure(Error.JobWriteFailed);
            }

            context.Logger.LogTrace("Uploaded work to room {RoomId}", header.RoomId);

            return Result<Error>.Success();
        }
        else
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }
    }

    private static bool IsAllowedInRoom(IPEndPoint sender, Room room, ref PacketHeader header)
    {
        return header.Type == PacketType.JoinRoom || room.PlayerHolder.FindPlayerByHost(sender) != null;
    }
}
