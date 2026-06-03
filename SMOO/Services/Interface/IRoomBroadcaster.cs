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
    IPacketPendingStore AckPacketStore { get; }

    Result<Error> Broadcast(Room room, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAck(Room room, in PacketAckBroadcastRequest request);

    Result<Error> BroadcastExcept(Room room, IPEndPoint sender, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAckExcept(Room room, Player sender, in PacketAckBroadcastRequest request);

    Result<Error> BroadcastExceptWith(Room room, IPEndPoint sender, ReadOnlySpan<byte> senderPayload, ReadOnlySpan<byte> broadcastPayload);
    Result<Error> BroadcastAckExceptWith(Room room, Player sender, in PacketAckBroadcastRequest playerRequest, in PacketAckBroadcastRequest broadcastRequest);

    Task Shutdown();
}
