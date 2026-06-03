using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Packet packet, ServerContext context)
    {
        Result<PacketHeader, Error> headerResult = PacketParser.ParseHeader(packet.RentedBuffer);
        if (headerResult.IsFailed)
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }

        PacketHeader header = headerResult.Data;
        if (header.Type == PacketType.Ping)
        {
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            return context.PacketSender.Send(packet.Sender, packet.RentedBuffer);
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        ParsedPacket roomPacket = new ParsedPacket()
        {
            Sender = packet.Sender,
            RentedBuffer = packet.RentedBuffer,
        };

        room.Packets.Writer.TryWrite(roomPacket);

        return Result<Error>.Success();
    }
}
