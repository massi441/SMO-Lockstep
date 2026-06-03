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
    IPendingPacketStore PendingPacketStore { get; }

    Result<Error> Broadcast(Room room, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAck(Room room, PacketBroadcastRequest request);

    Result<Error> BroadcastExcept(Room room, IPEndPoint sender, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAckExcept(Room room, Player sender, PacketBroadcastRequest request);

    Result<Error> BroadcastExceptWith(Room room, IPEndPoint sender, ReadOnlySpan<byte> senderPayload, ReadOnlySpan<byte> broadcastPayload);
    Result<Error> BroadcastAckExceptWith(Room room, Player sender, PacketBroadcastRequest playerRequest, PacketBroadcastRequest broadcastRequest);

    Task Shutdown();
}
