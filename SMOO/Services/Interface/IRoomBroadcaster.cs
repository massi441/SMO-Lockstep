using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IRoomBroadcaster
{
    /// <summary>
    /// The store of packets that need to be acked by clients
    /// </summary>
    IReliablePacketStore ReliablePacketStore { get; }

    void Broadcast(Room room, ReadOnlySpan<byte> payload);
    void BroadcastReliably(Room room, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries);

    void BroadcastExcept(Room room, Player ignoredPlayer, ReadOnlySpan<byte> payload);
    void BroadcastReliablyExcept(Room room, Player ignoredPlayer, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries);

    void BroadcastExceptWith(Room room, ReadOnlySpan<byte> roomPayload, Player ignoredPlayer, ReadOnlySpan<byte> ignoredPlayerPayload);
    void BroadcastReliablyExceptWith(Room room, RentedBuffer roomPayload, Player ignoredPlayer, RentedBuffer ignoredPlayerPayload, byte maxRetries = Config.MaxRetries);

    Task Shutdown();
}
