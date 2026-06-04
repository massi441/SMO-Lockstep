using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

internal readonly struct ReliablePacketRequest
{
    public Player Receiver { get; init; }
    public RentedBuffer RentedPayload { get; init; }
    public required byte MaxRetries { get; init; }
}

internal readonly struct ReliablePacketBroadcastRequest
{
    public RentedBuffer RentedPayload { get; init; }
    public required byte MaxRetries { get; init; }
}
