using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Socket socket, Payload packet, ServiceProvider serviceProvider)
    {
        Result<PacketHeader, Error> headerResult = PacketParser.ParseHeader(packet.Buffer);

        if (headerResult.IsSuccess)
        {
            PacketHeader header = headerResult.Data;
            if (header.PayloadSize == 0)
            {
                return Result<Error>.Failure(Error.EmptyPayload);
            }

            IPacketHandler? packetHandler = PacketHandlerFactory.CreateHandler(header.Type, serviceProvider);
            if (packetHandler == null)
            {
                return Result<Error>.Failure(Error.NoPacketHandler);
            }

            Payload packetPayload = new Payload(packet, PacketHeader.SizeOf());

            return packetHandler.Handle(socket, packetPayload);
        }
        else
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }
    }
}
