using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly ILogger _logger;

    public PacketConnectHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Result<Error> Handle(ReadOnlySpan<byte> payload)
    {
        _logger.LogInformation("Connection Packet Received");
        return Result<Error>.Success();
    }
}
