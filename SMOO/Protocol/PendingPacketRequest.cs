using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

internal readonly struct PendingPacketRequest
{
    public required Player Receiver { get; init; }
    public required RentedBuffer RentedPayload { get; init; }
    public byte MaxRetries { get; init; } = Config.MaxRetries;

    public PendingPacketRequest()
    {

    }
}

internal readonly struct PacketBroadcastRequest
{
    public required RentedBuffer RentedPayload { get; init; }
    public byte MaxRetries { get; init; } = Config.MaxRetries;

    public PacketBroadcastRequest() 
    {
        
    }
}
