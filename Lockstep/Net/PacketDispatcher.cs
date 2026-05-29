using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Payload packet, ServiceProvider serviceProvider)
    {
        Result<PacketHeader, Error> headerResult = PacketParser.ParseHeader(packet.Buffer.Span);

        if (headerResult.IsSuccess)
        {
            PacketHeader header = headerResult.Data;
            if (header.PayloadSize == 0)
            {
                return Result<Error>.Failure(Error.EmptyPayload);
            }

            if (!serviceProvider.Rooms.TryGetValue(header.RoomId, out Room? room))
            {
                return Result<Error>.Failure(Error.InvalidRoomId);
            }

            IPacketHandler? packetHandler = PacketHandlerFactory.CreateHandler(header.Type, serviceProvider);
            if (packetHandler == null)
            {
                return Result<Error>.Failure(Error.NoPacketHandler);
            }

            Payload packetPayload = new Payload(packet, PacketHeader.SizeOfSender());

            return packetHandler.Handle(room, packetPayload);
        }
        else
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }
    }
}
