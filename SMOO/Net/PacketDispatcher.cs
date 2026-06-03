using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Packet packet, ServerContext context)
    {
        Result<Error> headerResult = PacketParser.ParseHeader(packet);
        if (headerResult.IsFailed)
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }

        ref PacketHeader header = ref packet.Header;
        if (header.Type == PacketType.Ping)
        {
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            return context.PacketSender.Send(packet.Sender, packet.RentedBuffer.Span);
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        room.Packets.Writer.TryWrite(packet);

        return Result<Error>.Success();
    }
}
