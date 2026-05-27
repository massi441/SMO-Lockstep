using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketHealthCheckHandler : IPacketHandler
{
    private readonly ILogger _logger;

    public PacketHealthCheckHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Result<Error> Handle(ReadOnlySpan<byte> payload)
    {
        _logger.LogInformation("HealthCheck Packet Received");
        return Result<Error>.Success();
    }
}