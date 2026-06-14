using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketDefaultHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Default Packet Handler Involed for packet type: {PacketType}", packet.Header.Type);

        packet.RentedBuffer.Return();
    }
}
