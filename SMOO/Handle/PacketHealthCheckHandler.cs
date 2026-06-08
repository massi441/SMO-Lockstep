using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal class PacketHealthCheckHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketHealthCheckHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    public void Handle(ParsedPacket packet, Room room)
    {
        _context.Logger.LogTrace("Health check accepted");
        _context.PacketSender.Send(packet.SenderIp, packet.RentedBuffer.UsedSpan);
    }
}
