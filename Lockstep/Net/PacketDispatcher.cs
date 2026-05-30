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
            if (header.PayloadSize == 0)
            {
                return Result<Error>.Failure(Error.EmptyPayload);
            }

            Room? room = context.RoomHolder.GetRoom(header.RoomId);
            if (room == null)
            {
                return Result<Error>.Failure(Error.InvalidRoomId);
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
}
