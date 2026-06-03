using SMOO.Protocol;
using SMOO.Server;
using Microsoft.Extensions.Logging;

namespace SMOO.Net;

internal class PacketHealthCheckHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketHealthCheckHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    public void Handle(Packet packet, Room room)
    {
        _context.Logger.LogTrace("Health check accepted");
        _context.PacketSender.Send(packet.Sender, packet.Payload);
    }
}
