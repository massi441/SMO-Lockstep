using Lockstep.Client;

namespace Lockstep.Net;

internal readonly struct PacketPendingRequest
{
    public required Player Receiver { get; init; }
    public required byte[] Payload { get; init; }
    public required byte MaxRetries { get; init; }
}

internal readonly struct PacketAckBroadcastRequest
{
    public required byte[] Payload { get; init; }
    public required byte MaxRetries { get; init; }
}
