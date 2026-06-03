using System.Net;
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

    Result<Error> Broadcast(Room room, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastReliably(Room room, ReliablePacketBroadcastRequest request);

    Result<Error> BroadcastExcept(Room room, IPEndPoint sender, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastReliablyExcept(Room room, Player sender, ReliablePacketBroadcastRequest request);

    Result<Error> BroadcastExceptWith(Room room, IPEndPoint sender, ReadOnlySpan<byte> senderPayload, ReadOnlySpan<byte> broadcastPayload);
    Result<Error> BroadcastReliablyExceptWith(Room room, Player sender, ReliablePacketBroadcastRequest playerRequest, ReliablePacketBroadcastRequest broadcastRequest);

    Task Shutdown();
}
