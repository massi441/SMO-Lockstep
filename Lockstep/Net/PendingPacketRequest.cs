using System.Net;

namespace Lockstep.Net;

internal readonly struct PendingPacketRequest
{
    public IPEndPoint Receiver { get; init; }
    public byte[] Payload { get; init; }
    public byte MaxRetries { get; init; }
}
