using System.Text;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly ServerContext _context;

    private ILogger Logger => _context.Logger;
    private IPacketSender PacketSender => _context.PacketSender;

    public PacketConnectHandler(ServerContext context)
    {
        _context = context;
    }

    public Result<Error> Handle(Room room, Packet packet)
    {
        Logger.LogInformation("Connection Packet Received");
        string message = "Successfully Connected miller maggot";
        Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(message)];
        Encoding.UTF8.GetBytes(message, bytes);
        PacketSender.Send(bytes, packet.Payload.Sender);
        return Result<Error>.Success();
    }
}
