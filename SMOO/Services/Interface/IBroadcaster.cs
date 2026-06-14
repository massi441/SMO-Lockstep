using SMOO.Client;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IBroadcaster
{
    /// <summary>
    /// The store of packets that need to be acked by clients
    /// </summary>
    IReliablePacketStore ReliablePacketStore { get; }
    void Broadcast(Player[] players, RentedBuffer buffer);
    void BroadcastReliably(Player[] players, RentedBuffer buffer, byte maxRetries = Config.MaxRetries);
    Task Shutdown();
}
