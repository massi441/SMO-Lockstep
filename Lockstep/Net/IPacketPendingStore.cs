using System.Collections.Concurrent;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketPendingStore
{
    public ConcurrentDictionary<ushort, PacketPending> PendingPackets { get; }
    public Result<Error> UploadPacket(in PacketPendingRequest request);
    public PacketPending? RemovePacket(ushort sequenceNumber);
}
