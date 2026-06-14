using Microsoft.Extensions.Logging;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketHealthCheckHandler : IPacketHandler
{
    public static ushort MinPayloadSize => 0;
    public static ushort MaxPayloadSize => 0;

    public static void Handle(ParsedPacket packet, Room room, ServerContext context)
    {
        context.Logger.LogTrace("Health check accepted");
        context.PacketSender.SendTo(packet.SenderIp, packet.RentedBuffer.UsedSpan);

        packet.RentedBuffer.Return();
    }
}
