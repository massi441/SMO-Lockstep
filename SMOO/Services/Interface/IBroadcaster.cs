using SMOO.Enumerator;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IBroadcaster
{
    /// <summary>
    /// The store of packets that need to be acked by clients
    /// </summary>
    IReliablePacketStore ReliablePacketStore { get; }
    void Broadcast<TEnumerator>(TEnumerator players, RentedBuffer buffer) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    void BroadcastReliably<TEnumerator>(TEnumerator players, RentedBuffer buffer, byte maxRetries = Config.MaxRetries) where TEnumerator : IPlayerEnumerator<TEnumerator>, allows ref struct;
    Task Shutdown();
}
