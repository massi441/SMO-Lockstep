using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

internal readonly struct ReliablePacketRequest
{
    public Player Receiver { get; init; }
    public RentedBuffer RentedPayload { get; init; }
    public byte MaxRetries { get; init; } = Config.MaxRetries;

    public ReliablePacketRequest(Player receiver, RentedBuffer payload)
    {
        Receiver = receiver;
        RentedPayload = payload;
    }
}

internal readonly struct ReliablePacketBroadcastRequest
{
    public RentedBuffer RentedPayload { get; init; }
    public byte MaxRetries { get; init; } = Config.MaxRetries;

    public ReliablePacketBroadcastRequest(RentedBuffer payload)
    {
        RentedPayload = payload;
    }
}
