using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal interface IRoomNotifier
{
    Result<Error> NotifyAll(ReadOnlySpan<byte> payload);
    Result<Error> NotifyOthers(ReadOnlySpan<byte> payload, Player sender);
    Result<Error> NotifyOthers(ReadOnlySpan<byte> payload, Player sender, ReadOnlySpan<byte> senderPayload);
    Task Shutdown();

    IPacketPendingStore AckPacketStore { get; }
}
