using System.Text;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Net;

internal class PacketConnectHandler : IPacketHandler
{
    private readonly IPacketSender _packetSender;
    private readonly ILogger _logger;

    public PacketConnectHandler(IPacketSender packetSender, ILogger logger)
    {
        _packetSender = packetSender;
        _logger = logger;
    }

    public Result<Error> Handle(Room room, Payload packetPayload)
    {
        _logger.LogInformation("Connection Packet Received");
        string message = "Successfully Connected miller maggot";
        Span<byte> bytes = stackalloc byte[Encoding.UTF8.GetByteCount(message)];
        Encoding.UTF8.GetBytes(message, bytes);
        _packetSender.Send(bytes, packetPayload.Sender);
        return Result<Error>.Success();
    }
}
