using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal interface IRoomBroadcaster
{
    /// <summary>
    /// Returns the store of packets that need to be acked by clients
    /// </summary>
    IPacketPendingStore AckPacketStore { get; }

    Result<Error> Broadcast(Room room, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAck(Room room, in PacketAckBroadcastRequest request);

    Result<Error> BroadcastExcept(Room room, Player sender, ReadOnlySpan<byte> payload);
    Result<Error> BroadcastAckExcept(Room room, Player sender, in PacketAckBroadcastRequest request);

    Result<Error> BroadcastExceptWith(Room room, Player sender, ReadOnlySpan<byte> senderPayload, ReadOnlySpan<byte> broadcastPayload);
    Result<Error> BroadcastAckExceptWith(Room room, Player sender, in PacketAckBroadcastRequest playerRequest, in PacketAckBroadcastRequest broadcastRequest);

    Task Shutdown();
}
