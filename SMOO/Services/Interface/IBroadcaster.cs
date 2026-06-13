using SMOO.Client;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IBroadcaster
{
    /// <summary>
    /// The store of packets that need to be acked by clients
    /// </summary>
    IReliablePacketStore ReliablePacketStore { get; }

    void Broadcast(Player[] players, ReadOnlySpan<byte> payload);
    void BroadcastReliably(Player[] players, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries);

    void BroadcastExcept(Player[] players, Player ignoredPlayer, ReadOnlySpan<byte> payload);
    void BroadcastReliablyExcept(Player[] players, Player ignoredPlayer, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries);

    void BroadcastExceptWith(Player[] players, ReadOnlySpan<byte> roomPayload, Player ignoredPlayer, ReadOnlySpan<byte> ignoredPlayerPayload);
    void BroadcastReliablyExceptWith(Player[] players, RentedBuffer roomPayload, Player ignoredPlayer, RentedBuffer ignoredPlayerPayload, byte maxRetries = Config.MaxRetries);

    Task Shutdown();
}
