using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Packet packet)
    {
        Result<PacketHeader, Error> headerResult = PacketParser.ParseHeader(packet.Buffer);

        if (headerResult.IsSuccess)
        {
            PacketHeader header = headerResult.Data;
            if (header.PayloadSize == 0)
            {
                return Result<Error>.Failure(Error.EmptyPayload);
            }

            IPacketHandler packetHandler = PacketHandlerFactory.CreateHandler(header.Type);

            ReadOnlySpan<byte> packetPaylod = packet.Buffer[PacketHeader.SizeOf()..];

            return packetHandler.Handle(packetPaylod);
        }
        else
        {
            return Result<Error>.Failure(headerResult.Error!.Value);
        }
    }
}
