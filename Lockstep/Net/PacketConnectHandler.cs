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

    public Result<Error> Handle(Packet packet, Room room)
    {
        Logger.LogInformation("Connection Packet Received");
        return Result<Error>.Success();
    }
}
