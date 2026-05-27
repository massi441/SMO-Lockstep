using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketPlayerInputHandler : IPacketHandler
{
    private readonly ILogger _logger;

    public PacketPlayerInputHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Result<Error> Handle(ReadOnlySpan<byte> payload)
    {
        _logger.LogInformation("PlayerInput Packet Received");
        return Result<Error>.Success();
    }
}
