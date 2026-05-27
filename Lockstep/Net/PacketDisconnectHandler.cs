using System.Net.Sockets;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketDisconnectHandler : IPacketHandler
{
    private readonly ILogger _logger;

    public PacketDisconnectHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Result<Error> Handle(Payload packetPayload)
    {
        _logger.LogInformation("Disconnect Packet Received");
        return Result<Error>.Success();
    }
}