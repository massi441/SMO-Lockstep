using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly ClientHolder _clientHolder;
    private readonly ILogger _logger;

    public PacketConnectHandler(ClientHolder clientHolder, ILogger logger)
    {
        _clientHolder = clientHolder;
        _logger = logger;
    }

    public Result<Error> Handle(Payload packetPayload)
    {
        _logger.LogInformation("Connection Packet Received");
        return Result<Error>.Success();
    }
}
