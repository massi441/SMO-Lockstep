using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal interface IRoomBroadcaster
{
    IPacketPendingStore AckPacketStore { get; }
    Result<Error> Broadcast(ReadOnlySpan<byte> payload);
    Result<Error> BroadcastExcept(ReadOnlySpan<byte> payload, Player sender);
    Result<Error> BroadcastExceptWith(ReadOnlySpan<byte> payload, Player sender, ReadOnlySpan<byte> senderPayload);
    bool TryGetPendingCommand(out Action? command);
    Task Shutdown();
}
