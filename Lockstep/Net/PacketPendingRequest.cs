using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal readonly struct PacketPendingRequest
{
    public required IPEndPoint Receiver { get; init; }
    public required byte[] Payload { get; init; }
    public required byte MaxRetries { get; init; }
    public Func<IPEndPoint, Result<Error>>? OnDropped { get; init; }
}
