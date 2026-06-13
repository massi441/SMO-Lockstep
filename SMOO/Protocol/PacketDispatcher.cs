using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace SMOO.Protocol;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Packet packet, ServerContext context)
    {
        // Potentially move header parsing logic in private function here
        Result<Error> headerResult = PacketParser.ParseHeader(packet, context);
        if (headerResult.IsFailed)
        {
            packet.RentedBuffer.Return();
            return Result<Error>.Failure(headerResult.Error!.Value);
        }

        ref PacketHeader header = ref packet.Header;
        if (header.Type == PacketType.Ping)
        {
            packet.RentedBuffer.Return();
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            return context.PacketSender.SendTo(packet.Sender, packet.RentedBuffer.UsedSpan);
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            packet.RentedBuffer.Return();
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        room.Packets.Writer.TryWrite(packet);

        return Result<Error>.Success();
    }
}
