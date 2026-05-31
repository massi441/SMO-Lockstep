using System.Net;
using Lockstep.Client;
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

        if (headerResult.IsFailed)
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }

        PacketHeader header = headerResult.Data;

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        Packet roomPacket = new Packet()
        {
            Header = header,
            Payload = new Payload(packet, PacketHeader.SizeOf()),
        };

        room.Packets.Writer.TryWrite(roomPacket);

        return Result<Error>.Success();
    }
}
